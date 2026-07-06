import { Ionicons } from "@expo/vector-icons";
import { LinearGradient } from "expo-linear-gradient";
import { BottomTabBarProps } from "@react-navigation/bottom-tabs";
import { StyleSheet, Text, TouchableOpacity, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { colors } from "../constants/colors";

const icons: Record<string, keyof typeof Ionicons.glyphMap> = {
  Dashboard: "home",
  Suspended: "alert-circle",
  Checker: "search",
};

export default function FloatingTabBar({
  state,
  descriptors,
  navigation,
}: BottomTabBarProps) {
  const insets = useSafeAreaInsets();

  return (
    <View style={[styles.container, { bottom: insets.bottom + 16 }]}>
      {state.routes.map((route, index) => {
        const { options } = descriptors[route.key];
        const isFocused = state.index === index;

        const onPress = () => {
          const event = navigation.emit({
            type: "tabPress",
            target: route.key,
            canPreventDefault: true,
          });
          if (!isFocused && !event.defaultPrevented) {
            navigation.navigate(route.name);
          }
        };

        const label = options.tabBarLabel?.toString() ?? route.name;

        return (
          <TouchableOpacity
            key={route.key}
            onPress={onPress}
            style={styles.tab}
            activeOpacity={0.7}
          >
            {isFocused ? (
              <LinearGradient
                colors={colors.tabBarGradient}
                start={{ x: 0, y: 0 }}
                end={{ x: 1, y: 1 }}
                style={styles.activePill}
              >
                <Ionicons
                  name={icons[route.name]}
                  size={22}
                  color={colors.white}
                />
                <Text style={styles.activeLabel}>{label}</Text>
              </LinearGradient>
            ) : (
              <View style={styles.inactiveTab}>
                <Ionicons
                  name={icons[route.name]}
                  size={22}
                  color={colors.tabInactive}
                />
                <Text style={styles.inactiveLabel}>{label}</Text>
              </View>
            )}
          </TouchableOpacity>
        );
      })}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    position: "absolute",
    left: 24,
    right: 24,
    height: 64,
    borderRadius: 32,
    backgroundColor: "rgba(255,255,255,0.95)",
    borderWidth: 1,
    borderColor: colors.border,
    flexDirection: "row",
    alignItems: "center",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.12,
    shadowRadius: 16,
    elevation: 12,
  },
  tab: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
  },
  activePill: {
    alignItems: "center",
    justifyContent: "center",
    paddingHorizontal: 20,
    paddingVertical: 8,
    borderRadius: 32,
    gap: 2,
  },
  activeLabel: {
    color: colors.white,
    fontSize: 10,
    fontWeight: "600",
  },
  inactiveTab: {
    alignItems: "center",
    gap: 2,
  },
  inactiveLabel: {
    color: colors.tabInactive,
    fontSize: 10,
  },
});
