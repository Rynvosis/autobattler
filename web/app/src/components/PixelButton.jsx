const TONES = {
  gold: "bg-gold text-void",
  dark: "bg-slate text-parchment",
  ghost: "bg-panel text-parchment/60",
};

export function PixelButton({ tone = "dark", className = "", ...props }) {
  return <button {...props} className={`pixel-btn ${TONES[tone]} ${className}`} />;
}
