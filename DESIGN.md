---
name: Contador desktop
description: Aplicación nativa de recuento para Windows
colors:
  ground: "#101C2E"
  panel: "#182D48"
  ink: "#FFF4DF"
  muted: "#BDCEE3"
  gold: "#FFC857"
  line: "#4F7295"
  pirate-red: "#6E2636"
  ready: "#A9D5A5"
  status-attention: "#F1CA76"
  row-alternate: "#14243A"
  row-selected: "#365779"
  help-surface: "#20364C"
  help-ink: "#E2D1AD"
  text-selection: "#695128"
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

Interfaz de noche para acompañar una sesión de juego, con azul marino, rojo pirata y dorado de tesoro. La moneda aportada por el usuario forma parte de la identidad local de v1.2.

**Key Characteristics:**
- Estado siempre en texto además del color.
- Segoe UI respeta la plataforma Windows.
- Moneda pirata integrada; sin descargas de imágenes, transiciones ornamentales ni datos ficticios.

## Colors

Azul marino para el fondo, rojo pirata para la navegación, panel azul oscuro, texto marfil y secundario azul claro. El dorado enlaza el total y la selección de navegación; el verde claro indica detección disponible. Los valores normativos están en el frontmatter, extraídos del XAML y de los estados de navegación y detección.

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

Botones con borde discreto; al pasar el puntero el borde es dorado. El foco de teclado añade un borde dorado de 2 unidades. Desactivados: opacidad 0.45. Navegación activa: fondo dorado y texto azul marino.

Campos oscuros con cursor dorado y selección tostada. Selector de cuenta, casillas y deslizador son controles WPF. Tabla de solo lectura, ordenable, filas de 48 unidades, fondos alternos y selección azul más clara.

El estado tiene título, detalle y acción. El panel de ayuda usa azul tonal y texto arena. La moneda se muestra a 112 × 112 unidades con proporciones conservadas; `Assets/pirate-shop-token.png` es un Resource WPF integrado, cargado mediante URI de componente y con nombre accesible. El estado de partida ocupa una línea dorada de 18 unidades y peso Bold antes del estado del lector. Las superficies específicas se describen en `.impeccable/surfaces/desktop.md`.

## Do's and Don'ts

### Do:
- Do mantener el estado en texto además del color.
- Do usar Segoe UI y los diálogos del sistema.

### Don't:
- Don't descargar imágenes en ejecución, añadir transiciones ornamentales ni datos ficticios.

## Iconografía 1.2.1

La moneda se distribuye en Assets/Contador.ico con siete tamaños (16 a 256). ApplicationIcon y Window.Icon usan el mismo recurso; el motor lo embebe como Contador.Token.ico y lo dibuja en el overlay mediante CreateIconFromResourceEx/DrawIconEx. Cada handle temporal se libera con DestroyIcon. Se verificó también el icono extraído por Windows del ejecutable publicado.
