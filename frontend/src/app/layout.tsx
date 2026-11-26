import type { Metadata } from "next";
import { Bricolage_Grotesque, Inter } from "next/font/google";
import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-inter",
  display: "swap",
});

const bricolage = Bricolage_Grotesque({
  subsets: ["latin"],
  weight: ["500", "600", "700"],
  variable: "--font-bricolage",
  display: "swap",
});

export const metadata: Metadata = {
  title: {
    default: "ASCENSO — Plataforma de becas",
    template: "%s · ASCENSO",
  },
  description:
    "Plataforma de gestión de becas de la Fundación Ascenso: postulaciones, programas, presupuesto y seguimiento académico.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es" className={`${inter.variable} ${bricolage.variable}`}>
      <body className="min-h-screen bg-[var(--color-bg-surface)] text-[var(--color-text-primary)]">
        {children}
      </body>
    </html>
  );
}
