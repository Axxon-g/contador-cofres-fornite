---
name: Contador desktop
description: Aplicación nativa de recuento para Windows
colors:
  ground: "#101615"
  panel: "#19221F"
  ink: "#F3F5ED"
  muted: "#B4C1B7"
  gold: "#F1CA76"
  line: "#35443B"
  pine: "#1B2B23"
  ready: "#A9D5A5"
  row-alternate: "#17201C"
  row-selected: "#3C503E"
  help-surface: "#223428"
  help-ink: "#D0DFC9"
  text-selection: "#73613C"
typography:
  body:
    fontFamily: Segoe UI
    fontSize: "14px"
    fontWeight: 400
  headline:
    fontFamily: Segoe UI
    fontSize: "28px"
    fontWeight: 600
  title:
    fontFamily: Segoe UI
    fontSize: "20px"
    fontWeight: 600
  display:
    fontFamily: Segoe UI
    fontSize: "76px"
    fontWeight: 600
  label:
    fontFamily: Segoe UI
    fontSize: "12px"
    fontWeight: 400
rounded:
  control: "8px"
  panel: "10px"
spacing:
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
  xxl: "32px"
components:
  button:
    backgroundColor: "{colors.panel}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "11px 18px"
  navigation-selected:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.ground}"
    rounded: "{rounded.control}"
  input:
    backgroundColor: "{colors.ground}"
    textColor: "{colors.ink}"
    padding: "10px 12px"
  status-panel:
    backgroundColor: "{colors.panel}"
    rounded: "{rounded.panel}"
    padding: "13px 16px"
---

# Design System: Contador desktop

## Overview

**Creative North Star: "Operate"**

Operate: una aplicación de recuento para Windows. Navegación nativa, tabla ordenable, selección de carpetas y exportación con diálogos del sistema.

Interfaz de noche para acompañar una sesión de juego.

**Key Characteristics:**
- Estado siempre en texto además del color.
- Segoe UI respeta la plataforma Windows.
- Sin imágenes externas, transiciones ornamentales ni datos ficticios.

## Colors

Verde carbón para el fondo, pino para la navegación, panel verde oscuro, texto claro y secundario apagado. El dorado enlaza el total y la selección de navegación; el verde claro indica detección disponible. Los valores normativos están en el frontmatter, extraídos del XAML y de los estados de navegación y detección.

## Typography

Segoe UI respeta la plataforma Windows. Escala observada: 12/13/14/15/17/20/25/28 para información, 42/76 para cifras. Regular y SemiBold. La cifra total domina; la cifra de sesión queda en segundo plano.

Los valores `px` del frontmatter representan unidades independientes de dispositivo de WPF; no son píxeles físicos ni reglas CSS de la aplicación.

## Layout

Espaciado 8/12/16/24/32. Ventana mínima 860×640, principal 1120×780. Navegación de 200 unidades; área principal flexible con márgenes de 32 horizontales y 28 verticales. Contenido con desplazamiento donde hace falta. Overlay heredado, configurable e independiente.

## Elevation & Depth

Capas tonales y líneas suaves delimitan el contenido. No hay sombras ni animaciones ornamentales definidas en la ventana principal.

## Shapes

Radios de controles 8 y paneles 10 unidades. Los controles nativos conservan su comportamiento de Windows.

## Components

Botones con borde discreto; al pasar el puntero el borde es dorado. El foco de teclado añade un borde dorado de 2 unidades. Desactivados: opacidad 0.45. Navegación activa: fondo dorado y texto carbón.

Campos oscuros con cursor dorado y selección tostada. Selector de cuenta, casillas y deslizador son controles WPF. Tabla de solo lectura, ordenable, filas de 48 unidades, fondos alternos y selección verde más clara.

El estado tiene título, detalle y acción. El panel de ayuda usa verde tonal y texto claro. Las superficies específicas se describen en `.impeccable/surfaces/desktop.md`.

## Do's and Don'ts

### Do:
- Do mantener el estado en texto además del color.
- Do usar Segoe UI y los diálogos del sistema.

### Don't:
- Don't introducir imágenes externas, transiciones ornamentales ni datos ficticios.
