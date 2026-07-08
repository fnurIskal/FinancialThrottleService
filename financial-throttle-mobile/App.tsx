import { useEffect, useState } from "react";
import { Platform } from "react-native";
import { NavigationContainer } from "@react-navigation/native";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { SafeAreaProvider } from "react-native-safe-area-context";
import * as NavigationBar from "expo-navigation-bar";
import * as Notifications from "expo-notifications";
import * as Device from "expo-device";
import FloatingTabBar from "./components/FloatingTabBar";
import NotificationBanner, {
  NotificationBannerData,
} from "./components/NotificationBanner";
import DashboardScreen from "./screens/DashboardScreen";
import SuspendedScreen from "./screens/SuspendedScreen";
import CheckerScreen from "./screens/CheckerScreen";
import { api } from "./constants/api";
import Constants from "expo-constants";

const Tab = createBottomTabNavigator();

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
});
async function registerForPushNotifications(): Promise<string | null> {
  if (!Device.isDevice) {
    console.log("Physical device required");
    return null;
  }
  console.log("Starting push registration...");

  const { status: existingStatus } = await Notifications.getPermissionsAsync();
  console.log("Existing status:", existingStatus); // ← ekle

  let finalStatus = existingStatus;

  if (existingStatus !== "granted") {
    const { status } = await Notifications.requestPermissionsAsync();
    console.log("Requested status:", status); // ← ekle
    finalStatus = status;
  }

  console.log("Final status:", finalStatus); // ← ekle

  if (finalStatus !== "granted") {
    console.log("Permission denied");
    return null;
  }

  if (Platform.OS === "android") {
    await Notifications.setNotificationChannelAsync("suspended", {
      name: "Suspended Groups",
      importance: Notifications.AndroidImportance.HIGH,
      vibrationPattern: [0, 250, 250, 250],
      lightColor: "#DC2626",
    });
  }

  try {
    const projectId =
      Constants.expoConfig?.extra?.eas?.projectId ??
      Constants.easConfig?.projectId ??
      "00000000-0000-0000-0000-000000000000";

    const token = (await Notifications.getExpoPushTokenAsync({ projectId }))
      .data;
    console.log("📱 Expo Push Token:", token);

    if (!token.startsWith("ExponentPushToken[")) {
      console.log("Unexpected push token format, not registering:", token);
      return token;
    }

    await registerTokenWithBackend(token);

    return token;
  } catch (e) {
    console.log("Token error:", e); // ← ekle
    return null;
  }
}

async function registerTokenWithBackend(
  token: string,
  attempt = 1
): Promise<void> {
  try {
    await api.post("/register-token", { token });
    console.log("Push token registered with backend");
  } catch (e) {
    console.log(
      `Failed to register push token with backend (attempt ${attempt}):`,
      e
    );
    if (attempt < 3) {
      await new Promise((resolve) => setTimeout(resolve, attempt * 1000));
      return registerTokenWithBackend(token, attempt + 1);
    }
  }
}

export default function App() {
  const [banner, setBanner] = useState<NotificationBannerData | null>(null);

  useEffect(() => {
    if (Platform.OS !== "android") return;
    NavigationBar.setVisibilityAsync("hidden");
    NavigationBar.setBehaviorAsync("overlay-swipe");
  }, []);


  useEffect(() => {
    registerForPushNotifications();
  }, []);

  useEffect(() => {
    const receivedSub = Notifications.addNotificationReceivedListener(
      (notification) => {
        const { title, body, data } = notification.request.content;
        console.log("🔔 Notification received (foreground):", {
          title,
          body,
          data,
        });
        setBanner({
          title: title ?? "Notification",
          body: body ?? "",
        });
      }
    );

    const responseSub = Notifications.addNotificationResponseReceivedListener(
      (response) => {
        console.log(
          "👉 Notification tapped:",
          response.notification.request.content.data
        );
      }
    );

    return () => {
      receivedSub.remove();
      responseSub.remove();
    };
  }, []);

  return (
    <SafeAreaProvider>
      <NavigationContainer>
        <Tab.Navigator
          tabBar={(props) => <FloatingTabBar {...props} />}
          screenOptions={{ headerShown: false }}
        >
          <Tab.Screen name="Dashboard" component={DashboardScreen} />
          <Tab.Screen name="Suspended" component={SuspendedScreen} />
          <Tab.Screen name="Checker" component={CheckerScreen} />
        </Tab.Navigator>
      </NavigationContainer>
      <NotificationBanner
        notification={banner}
        onHide={() => setBanner(null)}
      />
    </SafeAreaProvider>
  );
}
