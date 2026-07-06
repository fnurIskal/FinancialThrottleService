import { StyleSheet, Text, View } from "react-native";
import { StatusBar } from "expo-status-bar";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { LinearGradient } from "expo-linear-gradient";
import { colors } from "../constants/colors";
import FrostedCard from "../components/FrostedCard";
import type { StatusResponse } from "../types";

function formatTime(iso?: string) {
  if (!iso) return "—";
  try {
    return new Date(iso).toLocaleTimeString();
  } catch {
    return iso;
  }
}

interface HeaderProps {
  status?: StatusResponse | null;
  loading?: boolean;
}

export default function Header({ status = null, loading = false }: HeaderProps) {
  const insets = useSafeAreaInsets();
  return (
    <View>
      <StatusBar style="light" />
      <View
        style={{
          borderBottomLeftRadius: 24,
          borderBottomRightRadius: 24,
          overflow: "hidden",
        }}
      >
        <LinearGradient
          colors={["#477DD7", "#BDE7B9"]}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 1 }}
          style={[styles.header, { paddingTop: insets.top + 60 }]}
        >
          <Text style={styles.greeting}>Hello,</Text>
          <Text style={styles.title}>Financial Throttle Service</Text>
          <FrostedCard
            style={styles.runningCard}
            title={loading ? "Loading..." : status?.isRunning ? "Running" : "Stopped"}
            subtitle={loading ? "" : `Worker started: ${formatTime(status?.workerStartedUtc)}`}
          />
        </LinearGradient>
      </View>
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
});
