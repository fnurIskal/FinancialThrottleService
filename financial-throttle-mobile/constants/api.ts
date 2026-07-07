import axios from "axios";

const LOCAL_IP = "192.168.1.197";

export const api = axios.create({
  baseURL: `http://${LOCAL_IP}:5059/api`,
  timeout: 15000,
  headers: { "Content-Type": "application/json" },
});
