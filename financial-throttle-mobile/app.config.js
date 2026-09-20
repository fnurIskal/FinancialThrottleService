// Public configuration only; never put server credentials in Expo extra or EXPO_PUBLIC_*.
module.exports = ({ config }) => ({
  ...config,
  ...(process.env.EXPO_OWNER ? { owner: process.env.EXPO_OWNER } : {}),
  android: {
    ...config.android,
    ...(process.env.GOOGLE_SERVICES_JSON ? { googleServicesFile: process.env.GOOGLE_SERVICES_JSON } : {}),
  },
  extra: {
    ...config.extra,
    eas: { projectId: process.env.EXPO_PUBLIC_EAS_PROJECT_ID || undefined },
  },
});