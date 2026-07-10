import { memo, useMemo, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { colors } from "../constants/colors";
import Card from "../components/Card";
import Badge from "../components/Badge";
import { fetchChecker } from "../services/endpoints";
import type { CheckerItemResult, CheckerResponse } from "../types";
import { StatusBar } from "expo-status-bar";
import { LinearGradient } from "expo-linear-gradient";
import { useSafeAreaInsets } from "react-native-safe-area-context";

type Filter = "all" | "processed" | "not_found";

function BoolIcon({ value }: { value: boolean }) {
  return (
    <Ionicons
      name={value ? "checkmark-circle" : "close-circle"}
      size={16}
      color={value ? "#22C55E" : "#F87171"}
    />
  );
}

function formatValue(value: number | null): string {
  if (value === null || value === undefined) return "—";
  return value.toLocaleString("tr-TR");
}

const CheckerItemCard = memo(function CheckerItemCard({
  item,
}: {
  item: CheckerItemResult;
}) {
  const [expanded, setExpanded] = useState(false);
  const [isTruncated, setIsTruncated] = useState(false);
  const codeDefText = `${item.itemQuarterlyCode ?? "—"} - ${
    item.originalDefinition || "—"
  }`;

  return (
    <Card style={styles.itemRow}>
      <View style={styles.itemMain}>
        {/* Görünmez ölçüm metni: gerçekte kaç satır tutacağını ölçer,
            sadece gerçekten tek satıra sığmayan tanımlarda toggle çıkar. */}
        <Text
          style={styles.itemCodeDefMeasure}
          onTextLayout={(e) => {
            if (e.nativeEvent.lines.length > 1) setIsTruncated(true);
          }}
        >
          {codeDefText}
        </Text>
        <TouchableOpacity
          activeOpacity={isTruncated ? 0.6 : 1}
          disabled={!isTruncated}
          onPress={() => setExpanded((v) => !v)}
          style={styles.itemCodeDefRow}
        >
          <Text
            style={styles.itemCodeDef}
            numberOfLines={expanded ? undefined : 1}
          >
            {codeDefText}
          </Text>
          {isTruncated && (
            <Ionicons
              name={expanded ? "chevron-up" : "chevron-down"}
              size={14}
              color={colors.textSecondary}
              style={styles.itemToggleIcon}
            />
          )}
        </TouchableOpacity>

        <Text style={styles.valueSectionLabel}>Item Value</Text>
        <View style={styles.valueRow}>
          <View style={styles.valueBox}>
            <View style={styles.valueBoxHeader}>
              <BoolIcon value={item.inQuarterly} />
              <Text style={styles.valueLabel}>Quarterly</Text>
            </View>
            <Text
              style={[
                styles.valueAmount,
                item.valuesMatch === false && styles.valueMismatch,
              ]}
            >
              {formatValue(item.quarterlyValue)}
            </Text>
          </View>
          <View style={styles.valueBox}>
            <View style={styles.valueBoxHeader}>
              <BoolIcon value={item.inQuarterlyOriginal} />
              <Text style={styles.valueLabel}>Production</Text>
            </View>
            <Text
              style={[
                styles.valueAmount,
                item.valuesMatch === false && styles.valueMismatch,
              ]}
            >
              {formatValue(item.originalValue)}
            </Text>
          </View>
        </View>
      </View>
      <Badge variant={item.status === "processed" ? "success" : "failure"}>
        {item.status === "not_found" ? "not found" : item.status}
      </Badge>
    </Card>
  );
});

export default function CheckerScreen() {
  const insets = useSafeAreaInsets();
  const [databaseName, setDatabaseName] = useState("");
  const [securityId, setSecurityId] = useState("");
  const [templateId, setTemplateId] = useState("");
  const [quarter, setQuarter] = useState("");
  const [itemQuarterlyCode, setItemQuarterlyCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<CheckerResponse | null>(null);
  const [filter, setFilter] = useState<Filter>("all");
  const [search, setSearch] = useState("");

  const canRun =
    !loading &&
    databaseName.trim() !== "" &&
    securityId !== "" &&
    templateId !== "" &&
    quarter !== "";

  const handleRun = async () => {
    if (!canRun) return;
    setLoading(true);
    setResult(null);
    try {
      const data = await fetchChecker(
        databaseName.trim(),
        parseInt(securityId, 10),
        parseInt(templateId, 10),
        parseInt(quarter, 10),
        itemQuarterlyCode.trim() ? parseInt(itemQuarterlyCode, 10) : undefined,
      );
      setResult(data);
      setFilter("all");
      setSearch("");
    } catch {
      Alert.alert(
        "Checker request failed",
        "Check your inputs or backend connection.",
      );
    } finally {
      setLoading(false);
    }
  };

  const items = result?.items ?? [];

  const filteredItems = useMemo(() => {
    return items.filter((i: CheckerItemResult) => {
      const matchesSearch =
        !search ||
        i.itemQuarterlyCode?.toString().includes(search) ||
        i.originalDefinition?.toLowerCase().includes(search.toLowerCase());
      const matchesFilter = filter === "all" || i.status === filter;
      return matchesSearch && matchesFilter;
    });
  }, [items, search, filter]);

  return (
    <View style={styles.container}>
      <View>
        <StatusBar style="light" />
        <View
          style={{
            marginBottom: 24,
          }}
        >
          <LinearGradient
            colors={["#477DD7", "#BDE7B9"]}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 1 }}
            style={[styles.header, { paddingTop: insets.top }]}
          >
            <Text style={styles.greeting}>Financial Throttle Service</Text>
            <Text style={styles.title}>Checker</Text>
          </LinearGradient>
        </View>
      </View>

      <KeyboardAvoidingView
        style={styles.keyboardAvoider}
        behavior={Platform.OS === "ios" ? "padding" : "height"}
      >
        <FlatList
          style={styles.scroll}
          contentContainerStyle={styles.content}
          data={result ? filteredItems : []}
          keyExtractor={(_, index) => index.toString()}
          renderItem={({ item }) => <CheckerItemCard item={item} />}
          keyboardShouldPersistTaps="handled"
          ListHeaderComponent={
            <View style={styles.listHeader}>
              <Card style={styles.formCard}>
                <Field
                  label="Database Name"
                  value={databaseName}
                  onChangeText={setDatabaseName}
                  placeholder="e.g. RAS_101"
                />
                <Field
                  label="Security ID"
                  value={securityId}
                  onChangeText={setSecurityId}
                  placeholder="e.g. 282"
                  keyboardType="numeric"
                />
                <Field
                  label="Template ID"
                  value={templateId}
                  onChangeText={setTemplateId}
                  placeholder="e.g. 21"
                  keyboardType="numeric"
                />
                <Field
                  label="Quarter"
                  value={quarter}
                  onChangeText={setQuarter}
                  placeholder="e.g. 202503"
                  keyboardType="numeric"
                />
                <Field
                  label="Item Quarterly Code (optional)"
                  value={itemQuarterlyCode}
                  onChangeText={setItemQuarterlyCode}
                  placeholder="e.g. 1105100227"
                  keyboardType="numeric"
                />

                <TouchableOpacity
                  style={[
                    styles.runButton,
                    !canRun && styles.runButtonDisabled,
                  ]}
                  disabled={!canRun}
                  onPress={handleRun}
                >
                  {loading ? (
                    <ActivityIndicator size="small" color={colors.white} />
                  ) : (
                    <Text style={styles.runButtonText}>Run</Text>
                  )}
                </TouchableOpacity>
              </Card>

              {result && (
                <>
                  <Card style={styles.summaryCard}>
                    <View style={styles.summaryRow}>
                      <Text style={styles.summaryValue}>
                        {result.totalCount}
                      </Text>
                      <Text style={styles.summaryLabel}>total</Text>
                    </View>
                    <View style={styles.summaryRow}>
                      <Ionicons
                        name="checkmark-circle"
                        size={16}
                        color="#22C55E"
                      />
                      <Text style={styles.summaryValue}>
                        {result.processedCount}
                      </Text>
                      <Text style={styles.summaryLabel}>processed</Text>
                    </View>
                    <View style={styles.summaryRow}>
                      <Ionicons name="close-circle" size={16} color="#F87171" />
                      <Text style={styles.summaryValue}>
                        {result.notFoundCount}
                      </Text>
                      <Text style={styles.summaryLabel}>not found</Text>
                    </View>
                  </Card>

                  <Card style={styles.filterCard}>
                    <View style={styles.searchBox}>
                      <Ionicons
                        name="search"
                        size={14}
                        color={colors.textSecondary}
                      />
                      <TextInput
                        style={styles.searchInput}
                        value={search}
                        onChangeText={setSearch}
                        placeholder="Search code or definition..."
                        placeholderTextColor={colors.textSecondary}
                      />
                    </View>
                    <View style={styles.chipRow}>
                      {(["all", "processed", "not_found"] as Filter[]).map(
                        (f) => (
                          <TouchableOpacity
                            key={f}
                            style={[
                              styles.chip,
                              filter === f && styles.chipActive,
                            ]}
                            onPress={() => setFilter(f)}
                          >
                            <Text
                              style={[
                                styles.chipText,
                                filter === f && styles.chipTextActive,
                              ]}
                            >
                              {f === "all"
                                ? "All"
                                : f === "processed"
                                  ? "Processed"
                                  : "Not Found"}
                            </Text>
                          </TouchableOpacity>
                        ),
                      )}
                    </View>
                  </Card>

                  <Text style={styles.resultsHeader}>
                    Showing {filteredItems.length} of {items.length}
                  </Text>
                </>
              )}
            </View>
          }
          ListEmptyComponent={
            result ? (
              <Text style={styles.emptyText}>
                No items match the selected filters.
              </Text>
            ) : null
          }
        />
      </KeyboardAvoidingView>
    </View>
  );
}

interface FieldProps {
  label: string;
  value: string;
  onChangeText: (text: string) => void;
  placeholder?: string;
  keyboardType?: "default" | "numeric";
}

function Field({
  label,
  value,
  onChangeText,
  placeholder,
  keyboardType,
}: FieldProps) {
  return (
    <View style={styles.field}>
      <Text style={styles.fieldLabel}>{label}</Text>
      <TextInput
        style={styles.input}
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor={colors.textSecondary}
        keyboardType={keyboardType}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    paddingTop: 16,
    paddingBottom: 32,
    paddingHorizontal: 20,
  },
  greeting: {
    color: "rgba(255,255,255,0.85)",
    fontSize: 14,
  },
  title: {
    color: "#FFFFFF",
    fontSize: 22,
    fontWeight: "600",
    marginTop: 2,
  },
  pageTitle: {
    fontSize: 22,
    fontWeight: "600",
    padding: 16,
    color: colors.textPrimary,
  },
  keyboardAvoider: {
    flex: 1,
  },
  scroll: {
    flex: 1,
  },
  content: {
    paddingHorizontal: 16,
    paddingBottom: 120,
    gap: 12,
  },
  listHeader: {
    gap: 12,
    marginBottom: 12,
  },
  formCard: {
    gap: 10,
  },
  field: {
    gap: 4,
  },
  fieldLabel: {
    fontSize: 12,
    fontWeight: "500",
    color: colors.textSecondary,
  },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 8,
    fontSize: 14,
    color: colors.textPrimary,
    backgroundColor: colors.white,
  },
  runButton: {
    backgroundColor: colors.action,
    borderRadius: 8,
    paddingVertical: 12,
    alignItems: "center",
    marginTop: 4,
  },
  runButtonDisabled: {
    opacity: 0.5,
  },
  runButtonText: {
    color: colors.white,
    fontWeight: "700",
    fontSize: 14,
  },
  summaryCard: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 16,
  },
  summaryRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
  },
  summaryValue: {
    fontSize: 14,
    fontWeight: "700",
    color: colors.textPrimary,
  },
  summaryLabel: {
    fontSize: 13,
    color: colors.textSecondary,
  },
  filterCard: {
    gap: 10,
  },
  searchBox: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 8,
    backgroundColor: colors.background,
  },
  searchInput: {
    flex: 1,
    fontSize: 13,
    color: colors.textPrimary,
    padding: 0,
  },
  chipRow: {
    flexDirection: "row",
    gap: 8,
  },
  chip: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "#E5E7EB",
  },
  chipActive: {
    backgroundColor: colors.action,
  },
  chipText: {
    fontSize: 12,
    fontWeight: "600",
    color: colors.textSecondary,
  },
  chipTextActive: {
    color: colors.white,
  },
  resultsHeader: {
    fontSize: 12,
    color: colors.textSecondary,
  },
  itemRow: {
    flexDirection: "row",
    alignItems: "flex-start",
    justifyContent: "space-between",
    gap: 8,
  },
  itemMain: {
    flex: 1,
  },
  itemCode: {
    fontSize: 14,
    fontWeight: "600",
    color: colors.textPrimary,
  },
  itemDefinition: {
    fontSize: 12,
    color: colors.textSecondary,
    marginTop: 2,
  },
  itemCodeDefRow: {
    flexDirection: "row",
    alignItems: "flex-start",
    gap: 4,
  },
  itemCodeDef: {
    flex: 1,
    fontSize: 13,
    fontWeight: "600",
    color: colors.textPrimary,
  },
  // itemCodeDef ile aynı yazı tipi/boyut, ama chevron ikonuna ayrılan
  // alan kadar sağdan boşluk bırakılıyor ki ölçüm gerçek genişliği yansıtsın.
  itemCodeDefMeasure: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 18,
    fontSize: 13,
    fontWeight: "600",
    opacity: 0,
  },
  itemToggleIcon: {
    marginTop: 2,
  },
  valueSectionLabel: {
    fontSize: 10,
    fontWeight: "700",
    color: colors.textSecondary,
    textTransform: "uppercase",
    letterSpacing: 0.4,
    marginTop: 10,
    marginBottom: 4,
  },
  valueRow: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 20,
  },
  valueBox: {
    gap: 2,
  },
  valueBoxHeader: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
  },
  valueLabel: {
    fontSize: 11,
    color: colors.textSecondary,
  },
  valueAmount: {
    fontSize: 13,
    fontWeight: "600",
    color: colors.textPrimary,
  },
  valueMismatch: {
    color: "#DC2626",
  },
  emptyText: {
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: "center",
    paddingVertical: 20,
  },
});
