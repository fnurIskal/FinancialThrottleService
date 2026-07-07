import { useCallback, useEffect, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Modal,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { colors } from "../constants/colors";
import Card from "../components/Card";
import Badge from "../components/Badge";
import {
  fetchSuspended,
  forceSendSuspended,
  retrySuspended,
} from "../services/endpoints";
import type { SuspendedGroup } from "../types";
import { StatusBar } from "expo-status-bar";
import { LinearGradient } from "expo-linear-gradient";
import { useSafeAreaInsets } from "react-native-safe-area-context";

function formatDate(iso?: string) {
  if (!iso) return "—";
  try {
    return new Date(iso).toLocaleString();
  } catch {
    return iso;
  }
}

export default function SuspendedScreen() {
  const insets = useSafeAreaInsets();
  const [groups, setGroups] = useState<SuspendedGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busyKey, setBusyKey] = useState<string | null>(null);
  const [forceSendTarget, setForceSendTarget] = useState<SuspendedGroup | null>(
    null,
  );

  const load = useCallback(async () => {
    try {
      const data = await fetchSuspended();
      setGroups(data.groups);
      setError(null);
    } catch {
      setError("Failed to load suspended groups");
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

  const keyOf = (g: SuspendedGroup) =>
    `${g.databaseName}-${g.securityId}-${g.templateId}`;

  const handleRetry = async (g: SuspendedGroup) => {
    const key = keyOf(g);
    setBusyKey(key);
    try {
      await retrySuspended(g.databaseName, g.securityId, g.templateId);
      await load();
    } catch {
      Alert.alert("Retry failed", `Could not retry ${g.securityCode}.`);
    } finally {
      setBusyKey(null);
    }
  };

  const handleForceSend = async (g: SuspendedGroup) => {
    setForceSendTarget(null);
    const key = keyOf(g);
    setBusyKey(key);
    try {
      await forceSendSuspended(g.databaseName, g.securityId, g.templateId);
      await load();
    } catch {
      Alert.alert(
        "Force send failed",
        `Could not force send ${g.securityCode}.`,
      );
    } finally {
      setBusyKey(null);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={colors.action} />
      </View>
    );
  }

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
            <Text style={styles.title}>Suspended</Text>
          </LinearGradient>
        </View>
      </View>

      <ScrollView
        style={styles.scroll}
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
      >
        {error ? (
          <Text style={styles.errorText}>{error}</Text>
        ) : groups.length === 0 ? (
          <Text style={styles.emptyText}>No suspended groups. All clear!</Text>
        ) : (
          groups.map((g) => {
            const key = keyOf(g);
            const busy = busyKey === key || g.retryInProgress;
            return (
              <Card key={key} style={styles.card}>
                <View style={styles.cardHeader}>
                  <View style={styles.cardHeaderMain}>
                    <Text style={styles.title}>{g.databaseName}</Text>
                    <Text style={styles.subtitle}>
                      {g.securityCode} · #{g.templateId}
                    </Text>
                  </View>
                  <Badge variant="failure">{g.failureCount} attempts</Badge>
                </View>

                <View style={styles.infoRow}>
                  <Text style={styles.infoLabel}>First error</Text>
                  <Text style={styles.infoValue}>
                    {formatDate(g.firstFailedUtc)}
                  </Text>
                </View>
                <View style={styles.infoRow}>
                  <Text style={styles.infoLabel}>Next retry</Text>
                  <Text style={styles.infoValue}>
                    {formatDate(g.nextRetryUtc)}
                  </Text>
                </View>
                {g.lastError ? (
                  <View style={styles.infoRow}>
                    <Text style={styles.infoLabel}>Failure reason</Text>
                    <Text style={styles.infoValue} numberOfLines={2}>
                      {g.lastError}
                    </Text>
                  </View>
                ) : null}

                <View style={styles.actions}>
                  <TouchableOpacity
                    style={[
                      styles.button,
                      styles.retryButton,
                      busy && styles.buttonDisabled,
                    ]}
                    disabled={busy}
                    onPress={() => handleRetry(g)}
                  >
                    <Text style={styles.retryButtonText}>
                      {busyKey === key ? "Retrying..." : "Manual Retry"}
                    </Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[
                      styles.button,
                      styles.forceButton,
                      busy && styles.buttonDisabled,
                    ]}
                    disabled={busy}
                    onPress={() => setForceSendTarget(g)}
                  >
                    <Text style={styles.forceButtonText}>Force Send</Text>
                  </TouchableOpacity>
                </View>
              </Card>
            );
          })
        )}
      </ScrollView>

      <Modal
        visible={!!forceSendTarget}
        transparent
        animationType="fade"
        onRequestClose={() => setForceSendTarget(null)}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalCard}>
            <View style={styles.modalHeader}>
              <Text style={styles.modalTitle}>Confirm Force Send</Text>
              <TouchableOpacity onPress={() => setForceSendTarget(null)}>
                <Text style={styles.modalClose}>×</Text>
              </TouchableOpacity>
            </View>

            <View style={styles.modalWarningBox}>
              <Text style={styles.modalWarningIcon}>⚠</Text>
              <View style={{ flex: 1 }}>
                <Text style={styles.modalWarningText}>
                  This will skip all validation and send directly.
                </Text>
                <Text style={styles.modalWarningTextBold}>
                  This action cannot be undone.
                </Text>
              </View>
            </View>

            {[
              { label: "Security", value: forceSendTarget?.securityCode },
              { label: "Database", value: forceSendTarget?.databaseName },
              {
                label: "Template ID",
                value: `#${forceSendTarget?.templateId}`,
              },
            ].map((row) => (
              <View key={row.label} style={styles.modalDetailRow}>
                <Text style={styles.modalDetailLabel}>{row.label}</Text>
                <Text style={styles.modalDetailValue}>{row.value}</Text>
              </View>
            ))}

            <View style={styles.modalActions}>
              <TouchableOpacity
                onPress={() => setForceSendTarget(null)}
                style={styles.modalCancelButton}
              >
                <Text style={styles.modalCancelButtonText}>Cancel</Text>
              </TouchableOpacity>
              <TouchableOpacity
                onPress={() =>
                  forceSendTarget && handleForceSend(forceSendTarget)
                }
                style={styles.modalForceButton}
              >
                <Text style={styles.modalForceButtonText}>Force Send</Text>
              </TouchableOpacity>
            </View>
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
  pageTitle: {
    fontSize: 22,
    fontWeight: "600",
    padding: 16,
    color: colors.textPrimary,
  },
  scroll: {
    flex: 1,
  },
  content: {
    paddingHorizontal: 16,
    paddingBottom: 120,
    gap: 12,
  },
  center: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: colors.background,
  },
  card: {
    gap: 8,
  },
  cardHeader: {
    flexDirection: "row",
    alignItems: "flex-start",
    justifyContent: "space-between",
  },
  cardHeaderMain: {
    flex: 1,
    paddingRight: 8,
  },
  title: {
    color: "#FFFFFF",
    fontSize: 22,
    fontWeight: "600",
    marginTop: 2,
  },
  subtitle: {
    fontSize: 12,
    color: colors.textSecondary,
    marginTop: 2,
  },
  infoRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: 8,
  },
  infoLabel: {
    fontSize: 12,
    color: colors.textSecondary,
  },
  infoValue: {
    fontSize: 12,
    color: colors.textPrimary,
    flexShrink: 1,
    textAlign: "right",
  },
  actions: {
    flexDirection: "row",
    gap: 8,
    marginTop: 4,
  },
  button: {
    flex: 1,
    paddingVertical: 10,
    borderRadius: 8,
    alignItems: "center",
  },
  buttonDisabled: {
    opacity: 0.5,
  },
  retryButton: {
    backgroundColor: colors.white,
    borderWidth: 1,
    borderColor: colors.action,
  },
  retryButtonText: {
    color: colors.action,
    fontSize: 13,
    fontWeight: "600",
  },
  forceButton: {
    backgroundColor: colors.danger,
  },
  forceButtonText: {
    color: colors.white,
    fontSize: 13,
    fontWeight: "600",
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
    maxWidth: 360,
  },
  modalHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
  },
  modalTitle: {
    fontSize: 16,
    fontWeight: "600",
    color: colors.textPrimary,
  },
  modalClose: {
    fontSize: 20,
    color: colors.textSecondary,
  },
  modalWarningBox: {
    backgroundColor: colors.failure,
    borderRadius: 8,
    padding: 12,
    flexDirection: "row",
    gap: 8,
    marginBottom: 16,
  },
  modalWarningIcon: {
    color: colors.danger,
    fontSize: 13,
  },
  modalWarningText: {
    color: colors.danger,
    fontSize: 13,
  },
  modalWarningTextBold: {
    color: colors.danger,
    fontSize: 13,
    fontWeight: "700",
  },
  modalDetailRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    paddingVertical: 10,
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
  },
  modalActions: {
    flexDirection: "row",
    gap: 12,
    marginTop: 20,
  },
  modalCancelButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: "center",
    backgroundColor: colors.white,
  },
  modalCancelButtonText: {
    color: "#374151",
    fontWeight: "500",
  },
  modalForceButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 8,
    backgroundColor: colors.danger,
    alignItems: "center",
  },
  modalForceButtonText: {
    color: colors.white,
    fontWeight: "600",
  },
});
