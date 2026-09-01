import { createRoot } from "react-dom/client";
// Only the font: pixel-retroui's index.css carries its own unlayered Tailwind preflight,
// which overrides this project's layered utilities (a gold button came out transparent).
import "pixel-retroui/dist/fonts.css";
import "react-tooltip/dist/react-tooltip.css";
import "./styles.css";
import App from "./App.jsx";

// No StrictMode: it double-invokes effects, which would start the battle replay twice.
createRoot(document.getElementById("root")).render(<App />);
