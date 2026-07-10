import axios from "axios";

// AVD emulator kullanırken 10.0.2.2 host makinenin localhost'una karşılık gelir.
// Gerçek cihazda test ederken bunu bilgisayarının LAN IP'sine geri al (örn. localhost).
const LOCAL_IP = "localhost";

export const api = axios.create({
  baseURL: `http://${LOCAL_IP}:5059/api`,
  timeout: 15000,
  headers: { "Content-Type": "application/json" },
});
