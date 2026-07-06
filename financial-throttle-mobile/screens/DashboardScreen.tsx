import { useCallback, useEffect, useState } from "react";
import {
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
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
              <Card
                key={`${g.databaseName}-${g.securityId}-${g.templateId}`}
                style={styles.row}
              >
                <View style={styles.rowMain}>
                  <Text style={styles.rowTitle}>{g.databaseName}</Text>
                  <Text style={styles.rowSubtitle}>
                    {g.securityCode} · #{g.templateId} · {g.itemCount} items
                  </Text>
                </View>
                <Badge variant={badge.variant}>{badge.label}</Badge>
              </Card>
            );
          })
        )}
      </ScrollView>
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
});
