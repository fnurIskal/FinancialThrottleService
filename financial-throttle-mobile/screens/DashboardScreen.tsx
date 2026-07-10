import { useCallback, useEffect, useState } from "react";
import {
  Modal,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { StatusBar } from "expo-status-bar";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { LinearGradient } from "expo-linear-gradient";
import { Ionicons } from "@expo/vector-icons";
import { colors } from "../constants/colors";
import Card from "../components/Card";
import Badge, { BadgeVariant } from "../components/Badge";
import FrostedCard from "../components/FrostedCard";
import { fetchQueue, fetchStatus } from "../services/endpoints";
import type { StatusResponse, WaitingGroup } from "../types";
import Header from "../components/Header";

function formatTime(iso?: string) {
  if (!iso) return "—";
  try {
    return new Date(iso).toLocaleTimeString();
  } catch {
    return iso;
  }
}

function statusBadge(status?: string): {
  label: string;
  variant: BadgeVariant;
} {
  if (status === "suspended") return { label: "Suspended", variant: "failure" };
  if (status === "waiting") return { label: "Waiting", variant: "waiting" };
  return { label: "Queued", variant: "success" };
}

function formatQuarter(quarter: number) {
  const s = String(quarter);
  if (s.length !== 6) return s;
  return `${s.slice(4, 6)}/${s.slice(0, 4)}`;
}

interface StatCardProps {
  icon: keyof typeof Ionicons.glyphMap;
  value: string | number;
  label: string;
  subtitle: string;
}

function StatCard({ icon, value, label, subtitle }: StatCardProps) {
  return (
    <Card style={styles.statCard}>
      <View style={styles.statIconCircle}>
        <Ionicons name={icon} size={16} color="#5B8DEF" />
      </View>
      <Text style={styles.statValue}>{value}</Text>
      <Text style={styles.statLabel}>{label}</Text>
      <Text style={styles.statSubtitle}>{subtitle}</Text>
    </Card>
  );
}

export default function DashboardScreen() {
  const insets = useSafeAreaInsets();
  const [status, setStatus] = useState<StatusResponse | null>(null);
  const [groups, setGroups] = useState<WaitingGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedGroup, setSelectedGroup] = useState<WaitingGroup | null>(
    null,
  );

  const load = useCallback(async () => {
    const [statusResult, queueResult] = await Promise.allSettled([
      fetchStatus(),
      fetchQueue(),
    ]);

    if (statusResult.status === "fulfilled") {
      setStatus(statusResult.value);
    }

    if (queueResult.status === "fulfilled") {
      setGroups(
        [...queueResult.value.groups]
          .sort((a, b) => (b.orderType ?? 0) - (a.orderType ?? 0))
          .slice(0, 5),
      );
      setError(null);
    } else {
      setError("Could not load queue — Worker may be down");
    }
  }, []);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = useCallback(async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  }, [load]);

  return (
    <View style={styles.container}>
      <Header status={status} loading={loading} />

      <ScrollView
        style={styles.scroll}
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
      >
        <View style={styles.statsGrid}>
          <StatCard
            icon="hourglass-outline"
            value={status?.queuedCount ?? "—"}
            label="Pending"
            subtitle="Groups in queue"
          />
          <StatCard
            icon="alert-circle-outline"
            value={status?.suspendedCount ?? "—"}
            label="Suspended"
            subtitle="Needs attention"
          />
          <StatCard
            icon="checkmark-done-outline"
            value={status?.processedThisCycle ?? "—"}
            label="Processed"
            subtitle="This cycle"
          />
          <StatCard
            icon="pulse-outline"
            value={formatTime(status?.lastHeartbeatUtc)}
            label="Heartbeat"
            subtitle="Last signal"
          />
        </View>

        <Text style={styles.sectionTitle}>Last 5 Queue Groups</Text>

        {error ? (
          <Text style={styles.errorText}>{error}</Text>
        ) : groups.length === 0 ? (
          <Text style={styles.emptyText}>
            Queue is empty — no groups waiting to be processed.
          </Text>
        ) : (
          groups.map((g) => {
            const badge = statusBadge(g.status);
            return (
              <TouchableOpacity
                key={`${g.databaseName}-${g.securityId}-${g.templateId}`}
                activeOpacity={0.7}
                onPress={() => setSelectedGroup(g)}
              >
                <Card style={styles.row}>
                  <View style={styles.rowMain}>
                    <Text style={styles.rowTitle}>{g.databaseName}</Text>
                    <Text style={styles.rowSubtitle}>
                      {g.securityCode} · #{g.templateId} · {g.itemCount} items
                    </Text>
                  </View>
                  <Badge variant={badge.variant}>{badge.label}</Badge>
                </Card>
              </TouchableOpacity>
            );
          })
        )}
      </ScrollView>

      <Modal
        visible={!!selectedGroup}
        transparent
        animationType="fade"
        onRequestClose={() => setSelectedGroup(null)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalCard}>
            <View style={styles.modalHeader}>
              <View style={{ flex: 1 }}>
                <Text style={styles.modalTitle}>
                  {selectedGroup?.securityCode}
                </Text>
                <Text style={styles.modalSubtitle}>
                  {selectedGroup?.databaseName} · #{selectedGroup?.templateId}
                </Text>
              </View>
              <TouchableOpacity onPress={() => setSelectedGroup(null)}>
                <Text style={styles.modalClose}>×</Text>
              </TouchableOpacity>
            </View>

            {selectedGroup ? (
              <Badge
                variant={statusBadge(selectedGroup.status).variant}
              >
                {statusBadge(selectedGroup.status).label}
              </Badge>
            ) : null}

            <ScrollView style={styles.modalItemsScroll}>
              {[
                { label: "Security ID", value: String(selectedGroup?.securityId) },
                {
                  label: "Order Type",
                  value: selectedGroup?.orderType?.toString() ?? "—",
                },
                {
                  label: "Wait Reason",
                  value: selectedGroup?.waitReason ?? "—",
                },
                {
                  label: "Item Count",
                  value: String(selectedGroup?.itemCount ?? 0),
                },
              ].map((row) => (
                <View key={row.label} style={styles.modalDetailRow}>
                  <Text style={styles.modalDetailLabel}>{row.label}</Text>
                  <Text style={styles.modalDetailValue}>{row.value}</Text>
                </View>
              ))}

              <Text style={styles.modalItemsTitle}>Items</Text>
              {selectedGroup?.items.map((item, idx) => (
                <View key={idx} style={styles.modalItemCard}>
                  <View style={styles.modalItemHeader}>
                    <Text style={styles.modalItemQuarter}>
                      {formatQuarter(item.quarter)}
                    </Text>
                    <Badge variant={item.isOriginal ? "info" : "neutral"}>
                      {item.isOriginal ? "Original" : "Restated"}
                    </Badge>
                  </View>
                  <View style={styles.modalDetailRow}>
                    <Text style={styles.modalDetailLabel}>Username</Text>
                    <Text style={styles.modalDetailValue}>
                      {item.username}
                    </Text>
                  </View>
                  <View style={styles.modalDetailRow}>
                    <Text style={styles.modalDetailLabel}>Disclosure ID</Text>
                    <Text style={styles.modalDetailValue}>
                      {item.disclosureId}
                    </Text>
                  </View>
                  <View style={styles.modalDetailRow}>
                    <Text style={styles.modalDetailLabel}>Table Type</Text>
                    <Text style={styles.modalDetailValue}>
                      {item.tableTypeId}
                    </Text>
                  </View>
                </View>
              ))}
            </ScrollView>
          </View>
        </View>
      </Modal>
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
    marginBottom: 16,
  },
  runningCard: {
    alignSelf: "stretch",
  },
  scroll: {
    flex: 1,
  },
  content: {
    padding: 16,
    paddingBottom: 120,
    gap: 12,
  },
  center: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: colors.background,
  },
  statsGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 12,
    marginBottom: 4,
  },
  statCard: {
    width: "47%",
  },
  statIconCircle: {
    width: 30,
    height: 30,
    borderRadius: 15,
    backgroundColor: colors.info,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 10,
  },
  statValue: {
    fontSize: 22,
    fontWeight: "700",
    color: colors.textPrimary,
  },
  statLabel: {
    fontSize: 14,
    fontWeight: "600",
    color: colors.textPrimary,
    marginTop: 2,
  },
  statSubtitle: {
    fontSize: 12,
    color: colors.textSecondary,
    marginTop: 2,
  },
  sectionTitle: {
    fontSize: 14,
    fontWeight: "700",
    color: colors.textPrimary,
  },
  row: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  rowMain: {
    flex: 1,
    paddingRight: 8,
  },
  rowTitle: {
    fontSize: 14,
    fontWeight: "700",
    color: colors.textPrimary,
  },
  rowSubtitle: {
    fontSize: 12,
    color: colors.textSecondary,
    marginTop: 2,
  },
  errorText: {
    color: "#B91C1C",
    fontSize: 13,
    textAlign: "center",
    paddingVertical: 24,
  },
  emptyText: {
    color: colors.textSecondary,
    fontSize: 13,
    textAlign: "center",
    paddingVertical: 24,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.4)",
    justifyContent: "center",
    alignItems: "center",
    padding: 24,
  },
  modalCard: {
    backgroundColor: colors.white,
    borderRadius: 16,
    padding: 24,
    width: "100%",
    maxWidth: 380,
    maxHeight: "80%",
  },
  modalHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "flex-start",
    marginBottom: 12,
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: "700",
    color: colors.textPrimary,
  },
  modalSubtitle: {
    fontSize: 12,
    color: colors.textSecondary,
    marginTop: 2,
  },
  modalClose: {
    fontSize: 20,
    color: colors.textSecondary,
  },
  modalItemsScroll: {
    marginTop: 12,
  },
  modalDetailRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: 8,
    borderTopWidth: 1,
    borderTopColor: "#F3F4F6",
  },
  modalDetailLabel: {
    color: colors.textSecondary,
    fontSize: 13,
  },
  modalDetailValue: {
    color: colors.textPrimary,
    fontSize: 13,
    fontWeight: "500",
    flexShrink: 1,
    textAlign: "right",
  },
  modalItemsTitle: {
    fontSize: 13,
    fontWeight: "700",
    color: colors.textPrimary,
    marginTop: 16,
    marginBottom: 8,
  },
  modalItemCard: {
    backgroundColor: "#F9FAFB",
    borderRadius: 12,
    padding: 12,
    marginBottom: 8,
  },
  modalItemHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 4,
  },
  modalItemQuarter: {
    fontSize: 14,
    fontWeight: "700",
    color: colors.textPrimary,
  },
});
