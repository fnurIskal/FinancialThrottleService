# Paylaşım öncesi güvenlik durumu

Yerel kaynak dosyalardaki SQL/MongoDB parolaları, Firebase Admin SDK özel anahtarı,
Firebase istemci yapılandırması, Expo cihaz tokenı, kişisel Expo proje bağlantısı
ve sunucu adresleri temizlendi. EF context sınıfları bağlantıyı artık DI üzerinden alır.
.env ve kimlik dosyaları Git/Docker kapsamı dışında tutulur. Gitignore, daha önce
izlenmiş dosyaları veya eski commitleri temizlemez.

## Çalıştırma

Docker için kökteki .env.example dosyasını .env olarak kopyalayın ve iki farklı,
güçlü parola girin. Mongo parolasında URI güvenli karakterler kullanın.
Docker Compose .env dosyasını okur; doğrudan dotnet run bu dosyayı okumaz.
Veritabanları için uygulamanın beklediği şema ayrıca kurulmalıdır.

Doğrudan .NET çalıştırırken aşağıdaki ortam değişkenlerini kendi ortamınızda ayarlayın:
- ConnectionStrings__DefaultConnection
- ConnectionStrings__RasStaj107
- ConnectionStrings__RasStaj32501
- ConnectionStrings__Ras107
- ConnectionStrings__Ras32501
- MongoDB__ConnectionString
- ExpoPushToken (isteğe bağlı; boşsa bildirim atlanır)

Bağlantı örneği: Server=localhost;Database=RAS_STAJ;Integrated Security=True;TrustServerCertificate=True;
Gerçek bağlantıları appsettings dosyalarına veya README örneklerine yazmayın.

Mobil uygulamada .env.example dosyasını .env olarak kopyalayın. API URL'sini
cihazın erişebildiği adrese ayarlayın. Bildirimler için kendi EAS proje kimliğinizi,
Expo hesabınızı ve gerekirse GOOGLE_SERVICES_JSON ile kendi google-services.json
dosyanızın yolunu sağlayın. Android package değerini Firebase uygulamanızla eşleştirin.
EXPO_PUBLIC_* değerleri uygulama paketinde görünür; sunucu sırları burada tutulmaz.
Firebase Admin SDK özel anahtarı mobil uygulamaya hiçbir zaman eklenmez.
Frontend için de .env.example dosyasından yerel .env oluşturabilirsiniz.

## Public yapmadan önce kalan zorunlu işlemler

1. Daha önce depoya konmuş Firebase servis hesabı anahtarını Google Cloud IAM'de
   iptal edin. SQL Server ve MongoDB hesap parolalarını değiştirin; erişim kayıtlarını kontrol edin.
2. Firebase istemci anahtarının kısıtlamalarını ve kullanımını kontrol edin;
   gerekiyorsa anahtarı yenileyin. Eski Expo cihaz tokenını sunucu kayıtlarından çıkarın.
3. Master dalının 39 commitlik geçmişi git-filter-repo ile tarandı ve temizlendi;
   35 commit yeniden yazıldı. Firebase anahtar dosyaları geçmişten çıkarıldı,
   eski gömülü parolalar ve cihaz tokenları kaldırıldı. Eski klonları tekrar
   göndermeyin; depoyu yeniden klonlayın. GitHub önbellekleri, eski commit
   bağlantıları, forklar, Actions çıktıları ve release ekleri ayrıca kontrol
   edilmelidir; erişilebilir eski içerik için GitHub Support ile görüşün.
4. GitHub'da temizlenmiş kaynakları ve geçmişi doğrulayıp secret scanning /
   push protection özelliklerini etkinleştirdikten sonra görünürlüğü değiştirin.

Kaynak ve geçmiş temizliği master dalı için hazırlanmıştır; depo görünürlüğü değiştirilmemiştir.
Kaynakların temizlenmesi, çalışan API'nin internete güvenle açılabileceği anlamına gelmez.
