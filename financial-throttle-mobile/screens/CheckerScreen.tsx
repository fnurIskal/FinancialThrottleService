import { useMemo, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
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
        <ScrollView
          style={styles.scroll}
          contentContainerStyle={styles.content}
        >
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
              style={[styles.runButton, !canRun && styles.runButtonDisabled]}
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
                  <Text style={styles.summaryValue}>{result.totalCount}</Text>
                  <Text style={styles.summaryLabel}>total</Text>
                </View>
                <View style={styles.summaryRow}>
                  <Ionicons name="checkmark-circle" size={16} color="#22C55E" />
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
                  {(["all", "processed", "not_found"] as Filter[]).map((f) => (
                    <TouchableOpacity
                      key={f}
                      style={[styles.chip, filter === f && styles.chipActive]}
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
                  ))}
                </View>
              </Card>

              <Text style={styles.resultsHeader}>
                Showing {filteredItems.length} of {items.length}
              </Text>

              {filteredItems.length === 0 ? (
                <Text style={styles.emptyText}>
                  No items match the selected filters.
                </Text>
              ) : (
                filteredItems.map((item, i) => (
                  <Card key={i} style={styles.itemRow}>
                    <View style={styles.itemMain}>
                      <Text style={styles.itemCode}>
                        {item.itemQuarterlyCode ?? "—"}
                      </Text>
                      <Text style={styles.itemDefinition} numberOfLines={2}>
                        {item.originalDefinition || "—"}
                      </Text>
                      <View style={styles.itemFlags}>
                        <View style={styles.itemFlag}>
                          <BoolIcon value={item.inQuarterly} />
                          <Text style={styles.itemFlagLabel}>Quarterly</Text>
                        </View>
                        <View style={styles.itemFlag}>
                          <BoolIcon value={item.inQuarterlyOriginal} />
                          <Text style={styles.itemFlagLabel}>Production</Text>
                        </View>
                      </View>
                    </View>
                    <Badge
                      variant={
                        item.status === "processed" ? "success" : "failure"
                      }
                    >
                      {item.status === "not_found" ? "not found" : item.status}
                    </Badge>
                  </Card>
                ))
              )}
            </>
          )}
        </ScrollView>
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
  itemFlags: {
    flexDirection: "row",
    gap: 12,
    marginTop: 6,
  },
  itemFlag: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
  },
  itemFlagLabel: {
    fontSize: 11,
    color: colors.textSecondary,
  },
  emptyText: {
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: "center",
    paddingVertical: 20,
  },
});
