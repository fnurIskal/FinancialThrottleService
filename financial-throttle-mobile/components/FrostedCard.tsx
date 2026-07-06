import { StyleSheet, Text, View, ViewStyle } from "react-native";

interface FrostedCardProps {
  title: string;
  subtitle?: string;
  style?: ViewStyle;
}

export default function FrostedCard({
  title,
  subtitle,
  style,
}: FrostedCardProps) {
  return (
    <View style={[styles.card, style]}>
      <Text style={styles.title}>{title}</Text>
      {subtitle ? <Text style={styles.subtitle}>{subtitle}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: "#9ABCDA",
    borderRadius: 16,
    borderColor: "rgba(255, 255, 255, 0.6)",
    paddingHorizontal: 18,
    paddingVertical: 16,
  },
  title: {
    fontSize: 22,
    fontWeight: "700",
    color: "#fff",
  },
  subtitle: {
    fontSize: 13,
    marginTop: 4,
    color: "#fff",
  },
});
