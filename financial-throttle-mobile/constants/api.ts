import axios from "axios";

// Set the API URL for your device in the untracked .env file.
export const api = axios.create({
  baseURL: process.env.EXPO_PUBLIC_API_URL || "http://localhost:5059/api",
  timeout: 15000,
  headers: { "Content-Type": "application/json" },
});