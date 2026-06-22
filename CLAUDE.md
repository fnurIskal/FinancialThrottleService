# CLAUDE.md — Financial Throttle Service Proje Bağlamı

Bu dosya, AI asistanların (Claude, Copilot, Cursor vb.) bu projeyi anlaması için hazırlanmıştır.
Projeye bağlamdan kopuk sorular sormak yerine bu dosyayı ilk olarak oku.

---

## Proje Nedir?

**Financial Throttle Service**, borsada işlem gören şirketlerin finans verilerini (bilanço, gelir tablosu, dipnotlar vb.) bir kuyruk üzerinden işleyip yayınlayan bir arka plan servisidir.

- **Orijinal teknoloji:** .NET Framework 4.5 Windows Service
- **Hedef teknoloji:** .NET 10 Worker Service (arka plan) + Web API + Frontend UI
- **Geliştirme kısıtı:** Gerçek şirket sunucularına bağlanılamaz. Bunun yerine **staj veritabanları** kullanılır.

### Staj Veritabanı Eşlemesi

| Üretim | Staj | Açıklama |
|---|---|---|
| `RAS_DMS` | `RAS_STAJ` | Kuyruk, e-posta, trace tabloları |
| `RAS_0` | `RAS_STAJ` | Source, TableType, TableTemplate2 |
| `RAS_UTIL` | `RAS_STAJ` | Heartbeat tablosu |
| `RAS_101` | `RAS_STAJ107` | Türkiye finans DB |
| `RAS_32501` | `RAS_STAJ32501` | MS-source finans DB |
| `RAS_601`, `RAS_801`, `RAS_501` | **Yok** | Stajda mevcut değil |

**Kod içinde tüm `"RAS_DMS"`, `"RAS_0"`, `"RAS_101"`, `"RAS_32501"` string'leri config'den okunmalı; staj ortamında staj DB isimleriyle eşlenmelidir.**

---

## Temel Kavramlar

### WaitingGroup
Kuyrukta bekleyen bir işin birimi. Şu 3 anahtar ile tanımlanır:
- `DatabaseName` — örn. `"RAS_101"` (Türkiye), `"RAS_601"` (Rusya)
- `SecurityId` — şirket kimliği (int), örn. THYAO = 282
- `TemplateId` — finansal tablo şablonu (int), örn. bilanço = 21

Bir grubun birden fazla **WaitingItem**'ı olabilir (farklı dönemler).

### WaitingItem
- `Quarter` — dönem, `YYYYMM` formatında. Örn. `202409` = Eylül 2024
- `IsOriginal` — orijinal veri mi, düzeltilmiş mi
- `DisclosureId` — kamuya açıklama kimliği

### TableTypeId
Bir finansal tablonun tipi:
- `1` = Çeyreklik ana veri
- `2` = TTM (Trailing Twelve Months)
- `3` = Dipnotlar
- `4` = Ekler

### TemplateTableTypeConfig
Bir grubun gönderilebilmesi için hangi `TableTypeId`'lerin hazır olması gerektiğini tanımlayan kural tablosu. Örneğin RAS_101 + TemplateId=21 için `[1, 2, 3]` gerekir.

### GroupRetryTracker
In-memory (servis restart'ta sıfırlanan) retry takip mekanizması:
- 10 başarısız deneme → grup **SUSPEND** edilir
- Suspend sonrası retry takvimi: 5dk → 15dk → 30dk → 60dk → 3sa → 6sa → 1gün

---

## Arayüzler (Interface'ler)

Yeni projede veri erişimi şu arayüzler üzerinden yapılır. Geliştirme ortamında dummy implementasyonlar kullanılır.

### IFinancialRepository
```csharp
public interface IFinancialRepository
{
    // Kuyrukta bekleyen tüm grupları döner
    Task<List<WaitingGroup>> GetAllWaitingGroupsAsync();

    // Bir grubun tableTypeId listesini döner
    Task<List<int>> GetTableTypeIdsAsync(string databaseName, int securityId,
        int quarter, int templateId, bool isOriginal);

    // FinancialTrace tablosundan disclosureQuarter döner
    Task<int> GetDisclosureQuarterAsync(int securityId, int disclosureId,
        string databaseName, int templateId);

    // Hazır itemi gönderir (SQL çalıştırır, kuyruktan siler)
    Task ExecuteSendAsync(string databaseName, int securityId, int quarter,
        int templateId, int disclosureId, bool isOriginal,
        List<int> tableTypeIds, bool isInflationTemplate);

    // Heartbeat yazar
    Task WriteHeartbeatAsync();

    // RAS_101 grupları için security code döner
    Task<Dictionary<int, string>> GetSecurityCodesAsync(int[] securityIds);

    // DuplicateTableTemplateMap haritasını döner (günlük cache)
    Task<Dictionary<(int securityId, int templateId), int>> GetDuplicateItemCodeMapAsync();

    // MS-source veritabanı id'lerini döner (günlük cache)
    Task<int[]> GetMsSourceIdsAsync();
}
```

### ISecurityPriorityClient
```csharp
public interface ISecurityPriorityClient
{
    // SecurityCode listesi için priority score'larını döner
    // {"THYAO": 1.0, "SASA": 0.5, "AKBNK": 0.0}
    Task<Dictionary<string, double>> GetPrioritiesAsync(string[] securityCodes);
}
```

### IFinancialTransactionApi
```csharp
public interface IFinancialTransactionApi
{
    // generateInflation API çağrısı — dönen SQL'i uygular
    Task<string> GenerateInflationAsync(string database, int quarter,
        int securityId, int templateId, string commands);

    // generateRestated API çağrısı — dönen SQL'i uygular
    Task<string> GenerateRestatedAsync(string database, int quarter,
        int securityId, int templateId, string commands);
}
```

---

## İş Mantığı Özeti

### Ana Döngü (her 30 saniyede bir)
```
1. Heartbeat yaz
2. Kuyruktan tüm bekleyen grupları çek
3. Grupları DatabaseName|SecurityId|TemplateId anahtarıyla grupla
4. Maksimum 5 grup paralel olarak işle (MaxParallelGroups)
5. Her grup için:
   a. TableTypeIds'i sorgula
   b. TemplateTableTypeConfig ile beklenen tipleri karşılaştır
   c. EvaluateSendCondition ile gönderim koşulunu kontrol et
   d. Hazırsa → ExecuteSend çalıştır
   e. Hazır değilse → grup ertelenir (triedKeys'e eklenir)
6. Hata alınırsa → GroupRetryTracker.RecordFailure
7. 10 başarısız deneme → grup suspend edilir, mail gönderilir
8. Normal döngü bitince → suspend grupları retry zamanı geldi mi kontrol et
```

### EvaluateSendCondition Kuralları
```
KOŞUL 1: Tüm beklenen TableTypeId'ler mevcut → GÖNDER

KOŞUL 2 (username != "boss" VEYA database != "RAS_101"):
  - Quarterly/QuarterlyOriginal'de o dönem için veri var → GÖNDER
  - Yoksa tableTypeId in (1,2,5) ile de kontrol et → GÖNDER / BEKLE

KOŞUL 3 (username == "boss" VE database == "RAS_101" VE disclosureQuarter > 1):
  - Hem `quarter` hem `disclosureQuarter` dönemleri için veri var → GÖNDER
  - Yoksa → BEKLE
```

### Öncelik Skoru (Sadece RAS_101)
```
Score = ApiScore + 1.0
  - ApiScore: 1.0 (ERTELITE+XU030), 0.5 (ERTELITE), 0.0 (diğer)
  - RAS_101 dışı gruplar: Score = 0.0
  - Eşit scorede → rastgele seç
```

---

## Dummy Data Örnekleri

### Örnek WaitingGroups (staj DB'sinden gelecek veriler)

> Aşağıdaki `databaseName` değerleri staj DB'lerini kullanır.

```json
[
  {
    "databaseName": "RAS_STAJ107",
    "securityId": 282,
    "templateId": 21,
    "securityCode": "THYAO",
    "items": [
      { "quarter": 202409, "isOriginal": false, "username": "boss", "disclosureId": 1001 }
    ]
  },
  {
    "databaseName": "RAS_STAJ107",
    "securityId": 292,
    "templateId": 21,
    "securityCode": "SASA",
    "items": [
      { "quarter": 202409, "isOriginal": false, "username": "analyst1", "disclosureId": 1002 }
    ]
  },
  {
    "databaseName": "RAS_STAJ32501",
    "securityId": 50,
    "templateId": 241,
    "securityCode": "MSSTOCK",
    "items": [
      { "quarter": 202412, "isOriginal": false, "username": "analyst2", "disclosureId": 2001 }
    ]
  },
  {
    "databaseName": "RAS_STAJ107",
    "securityId": 346,
    "templateId": 21,
    "securityCode": "AKBNK",
    "items": [
      { "quarter": 202409, "isOriginal": false, "username": "boss", "disclosureId": 1003 },
      { "quarter": 202406, "isOriginal": false, "username": "boss", "disclosureId": 1004 }
    ]
  }
]
```

### Örnek Security Priority Scores
```json
{
  "THYAO": 1.0,
  "SASA": 0.5,
  "AKBNK": 0.0,
  "MSSTOCK": 0.0
}
```

### Örnek TableTypeIds (RAS_STAJ'dan okunur)
Bir grubun WaitingFinancialTables'da sahip olduğu tableTypeId'ler:
```json
{
  "RAS_STAJ107|282|21|202409":    [1, 2, 3],
  "RAS_STAJ107|292|21|202409":    [1, 2],
  "RAS_STAJ32501|50|241|202412":  [1, 2]
}
```

### Örnek DuplicateItemCodeMap (RAS_STAJ'dan okunur)
```json
[
  { "securityId": 282, "templateId": 21, "fromTemplateId": 1 }
]
```

### TemplateTableTypeConfig — Staj DB İsimleriyle Kural Tablosu

`TemplateTableTypeConfig.cs` dosyasında veritabanı isimleri güncellenmeli:

```csharp
// ESKI → YENİ
new Rule("RAS_101",   ...)  →  new Rule("RAS_STAJ107",   ...)
new Rule("RAS_32501", ...)  →  new Rule("RAS_STAJ32501", ...)
```

Staj ortamında geçerli kurallar:

| Staj DB | TemplateId'ler | Gerekli TypeId'ler |
|---|---|---|
| `RAS_STAJ107` | 21, 281, 282 | [1, 2, 3] |
| `RAS_STAJ107` | 2, 5, 8, 11, 14, 31, 36 | [1, 2] |
| `RAS_STAJ107` | varsayılan | [1, 2, 3, 4] |
| `RAS_STAJ32501` | 241, 243, 251, 252 | [1, 2] |
| `RAS_STAJ32501` | 242, 247, 248, 249, 250 | [1, 2, 3] |

---

## Konfigürasyon (appsettings.Development.json)

```json
{
  "ThrottleOptions": {
    "MaxParallelGroups": 5,
    "WorkerIntervalSeconds": 30,
    "UseDummyData": true
  },
  "DatabaseNames": {
    "Management":  "RAS_STAJ",
    "Reference":   "RAS_STAJ",
    "Heartbeat":   "RAS_STAJ",
    "Turkey":      "RAS_STAJ107",
    "MsSource":    "RAS_STAJ32501"
  },
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=RAS_STAJ;Trusted_Connection=True;"
  },
  "SecurityPriorityApi": {
    "BaseUrl": "http://localhost:11080/",
    "TimeoutSeconds": 10
  },
  "FinancialTransactionApi": {
    "BaseUrl": "https://api.example.com/service-financialtransactionhub-test/",
    "TimeoutSeconds": 120
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FinancialThrottle": "Debug"
    }
  }
}
```

---

## Dikkat Edilmesi Gereken Kurallar

### Yapılacaklar
- [ ] Her task kendi bağımlılık scope'unu oluşturmalı (thread-safe)
- [ ] `HttpClient` için `IHttpClientFactory` kullan — doğrudan `new HttpClient()` kullanma
- [ ] `GroupRetryTracker` `ConcurrentDictionary` ile implement edilmeli
- [ ] `PeriodicTimer` kullanırken `CancellationToken` geçilmeli
- [ ] Log dosyaları için retention politikası uygulanmalı
- [ ] Tüm async metodlar `Async` suffix almalı ve `await` doğru kullanılmalı

### Yapılmayacaklar
- ❌ `Thread.Sleep()` kullanma → `await Task.Delay()` kullan
- ❌ `Task.WaitAll()` kullanma → `await Task.WhenAll()` kullan
- ❌ Statik `HttpClient` instance'ı doğrudan oluşturma
- ❌ `DataTable` kullanma → strongly-typed modeller kullan
- ❌ `ConfigurationManager.AppSettings` kullanma → `IConfiguration` kullan
- ❌ `System.Windows.Forms` referansı ekleme
- ❌ Gerçek veritabanı veya şirket sunucusuna bağlanma (geliştirme ortamında)

---

## Proje Dosya Haritası (Orijinal)

```
FinancialThrottleService.cs   ← Ana servis + iş mantığı (2270 satır)
  ├── Worker()                  Her 30sn tetiklenen döngü
  ├── SendWaitingFinancials()   Ana işleme metodu
  ├── ProcessWaitingGroup()     Tek grup işleme
  ├── EvaluateSendCondition()   Gönderim hazırlık kontrolü
  ├── ExecuteSend()             Fiziksel gönderim
  ├── GetNewAPI()               HTTP API çağrısı (generateInflation/Restated)
  ├── FootnoteDataGenerator()   Dipnot oluşturucu
  └── TriggerConsistencyCheck() Tutarlılık kontrol tetikleyici

GroupRetryTracker.cs          ← Retry/suspend state yönetimi (340 satır)
SecurityPriorityClient.cs     ← Priority HTTP API istemcisi (117 satır)
TemplateTableTypeConfig.cs    ← TableType kural tablosu (244 satır)
ServiceLogger.cs              ← Thread-safe dosya logger (769 satır)
CommandLogger.cs              ← Eski komut loggeri (91 satır, artık kullanılmıyor)
Service.Methods.cs            ← DMS sunucu istemcisi (597 satır)
Program.cs                    ← Giriş noktası (29 satır)
```

---

## Sık Sorulan Sorular

**S: `DMS Service` nedir?**  
C: DataManagementStudio'nun kendi sunucu katmanı. SQL'i doğrudan değil, XML protokolü üzerinden HTTP ile çalıştırıyor. Yeni projede bu katman tamamen kaldırılacak, yerine doğrudan repository arayüzü geçecek.

**S: `RAS_DMS`, `RAS_101`, `RAS_0` ne anlama geliyor?**  
C: Farklı SQL Server veritabanları. `RAS_DMS` = servis yönetim DB (kuyruk, e-posta, log). `RAS_101` = Türkiye finans veritabanı. `RAS_0` = ortak referans veritabanı. `RAS_601` = Rusya, `RAS_801` = Macaristan, `RAS_501` = Romanya.

**S: Bir grup neden "suspend" edilir?**  
C: Arka arkaya 10 kez başarısız olunca (hata veya sürekli "hazır değil" durumu). Suspend sonrası 5dk → 15dk → 30dk → 60dk → 3sa → 6sa → 1gün artan bekleme aralıklarıyla retry yapılır.

**S: `Quarter = 202409` ne demek?**  
C: 2024 yılı Eylül ayı (Q3). Format YYYYMM. Çeyrekler: Mart(03), Haziran(06), Eylül(09), Aralık(12).

**S: Inflation API ne zaman çağrılır?**  
C: `TemplateId` şu listede yer alıyorsa: `[1, 15, 16, 17, 18, 19, 21, 23, 31, 32, 33, 98, 192, 193, 194, 255, 256, 257, 281, 282]`. Enflasyon düzeltmesi gerektiren finansal tablo şablonları.

**S: Frontend ne yapacak?**  
C: Kuyruk durumunu, suspend edilmiş grupları, log dosyalarını gösteren bir web arayüzü. Backend API'yi sorgular. REST API üzerinden çalışır.

**S: Test nasıl yapılacak?**  
C: `DummyFinancialRepository` ile gerçek DB olmadan tüm iş mantığı test edilebilir. Worker'ı başlatıp kuyruktan işlenip işlenmediğini gözlemleyin. Geliştirme ortamında `"UseDummyData": true` config ile dummy implementasyonlar inject edilir.

---

## Geliştirme Başlangıç Noktaları

1. `IFinancialRepository` arayüzünü oluştur
2. `DummyFinancialRepository` ile dummy veri döndür (yukarıdaki örnekler)
3. `FinancialThrottleWorker` (BackgroundService) oluştur
4. `GroupRetryTracker`'ı `ConcurrentDictionary` ile port et
5. `TemplateTableTypeConfig` kurallarını doğrudan taşı (değişmez)
6. `SendConditionEvaluator`'ı ayrı bir sınıfa çıkar ve test yaz
7. Minimal API endpoint'lerini ekle
8. Frontend'i backend API'ye bağla
