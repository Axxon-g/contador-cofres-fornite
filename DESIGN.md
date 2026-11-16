---
name: Contador desktop
description: Herramienta nativa de recuento con evidencia por cuenta y jugador
colors:
  ground: "#10243B"
  panel: "#183D60"
  raised: "#285577"
  rail: "#80273C"
  ink: "#FFF5DC"
  muted: "#CCE1EE"
  gold: "#FFD05A"
  line: "#517C9D"
  ready: "#A6EBBA"
  sea: "#80DDEB"
  coral: "#FFAAC0"
typography:
  body:
    fontFamily: Segoe UI
    fontSize: "14px"
    fontWeight: 400
  headline:
    fontFamily: Segoe UI
    fontSize: "27px"
    fontWeight: 600
  title:
    fontFamily: Segoe UI
    fontSize: "19px"
    fontWeight: 600
  display:
    fontFamily: Segoe UI
    fontSize: "60px"
    fontWeight: 600
  label:
    fontFamily: Segoe UI
    fontSize: "12px"
    fontWeight: 400
rounded:
  control: "8px"
  selector: "7px"
  panel: "10px"
spacing:
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
components:
  button:
    backgroundColor: "{colors.panel}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "10px 16px"
  button-primary:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.ground}"
    rounded: "{rounded.control}"
    padding: "10px 16px"
  navigation-selected:
    backgroundColor: "{colors.gold}"
    textColor: "{colors.ground}"
    rounded: "{rounded.control}"
    padding: "11px 12px"
  input:
    backgroundColor: "{colors.panel}"
    textColor: "{colors.ink}"
    padding: "10px 12px"
---

# Design System: Contador desktop

## Overview

**Creative North Star: "Operate"**

Herramienta nativa de Windows para contar con evidencia y resolver tareas sin ruido. El usuario sustituyó expresamente la dirección sobria por una identidad pirata más viva, inspirada en el color de animación: rail rojo, mar azul, tesoro dorado, turquesa y coral. La moneda y el cofre aportados forman parte de esa identidad aprobada; el sistema sigue siendo una herramienta nativa clara y compacta.

Segoe UI, controles WPF y diálogos del sistema mantienen familiaridad de escritorio. La identidad procede de la moneda y el cofre integrados, las cifras doradas y los acentos marinos, sin transiciones ornamentales. Es una renovación aprobada desde código, sin composición aprobada por imagen.

**Key Characteristics:**
- Estado y procedencia expresados en texto además del color.
- Navegación compacta y espacio de trabajo flexible.
- Cuenta global, cifras personales y evidencia del equipo diferenciadas.
- Moneda local integrada y controles nativos de Windows.

## Colors

Los valores normativos del frontmatter proceden de los recursos compartidos de `src/Contador/App.xaml`.

### Primary
- **Dorado de tesoro:** acciones principales, cifras destacadas, presencia y fondo de navegación activa, con texto oscuro.

### Secondary
- **Rojo pirata:** rail de navegación recuperado por petición expresa.
- **Mar turquesa:** títulos de página.
- **Coral:** lema de marca.
- **Verde disponible:** estados positivos de detección, acompañados de texto.

### Neutral
- **Fondo nocturno:** raíz de ventana y tablas.
- **Panel marino:** contenedores, campos y filas alternas.
- **Elevado tonal:** filas seleccionadas, desplegables y ayudas.
- **Marfil:** lectura principal y foco visible.
- **Azul gris:** etiquetas, explicación y navegación inactiva.
- **Línea:** divisores y contornos.

**The Evidence Rule.** El color acompaña el significado textual; nunca convierte una estimación o mínimo en una cifra exacta.

## Typography

**Body Font:** Segoe UI, con resolución nativa de Windows.

Jerarquía funcional: encabezado de página 27, título compartido 19, cuerpo 14 y etiquetas 12. Títulos locales también usan 20 y 24; total personal 60, sesión 36, estimación visual 44 e informe 42. Regular y SemiBold. Las unidades `px` representan unidades independientes de dispositivo WPF, no píxeles físicos ni CSS. No se extrae altura de línea ni espaciado de letras inexistente.

## Layout

Ventana principal 1180 × 820, mínimo 860 × 640. Rail fijo de 184 unidades; contenido flexible con margen horizontal 28 y vertical 24. Cabecera persistente con título, tarea y selector de cuenta de 210 unidades. Estado compartido compacto separado por línea; mapa, informe y estadísticas usan su propio contexto.

Siete espacios: Resumen, Estadísticas, Historial, Equipo, Mapa y rutas, Informe y Ajustes. Desplazamiento vertical donde se requiere; tablas con desplazamiento horizontal automático. WrapPanel permite redistribuir grupos de controles. No hay puntos de ruptura CSS. El overlay heredado tiene composición y configuración independientes.

**The Task First Rule.** La primera vista presenta cuenta, tarea y estado compacto antes del detalle; la evidencia extensa se consulta en tablas o desplegables.

## Elevation & Depth

Capas tonales y divisores, sin sombras compartidas ni animación ornamental. Foco marfil de 2 unidades separado 3 del control; la acción de lectura visual usa separación de 4.

## Shapes

Botones con curvas de 8, selectores de 7 y paneles de 10 unidades. Moneda local integrada de 72 × 72, centrada sobre la marca en el rail, y cofre local de 56 × 56 antes del total personal de cofres. Ambos conservan proporción y nombre accesible; Assets/pirate-chest.png se compila como Resource WPF. El lema aparece en coral. Las referencias de animación orientan la paleta: no se incrustan pósteres ni se añade movimiento. Iconos de navegación de trazado vectorial de 16 unidades. Assets/Contador.ico mantiene la identidad para ventana, ejecutable y bandeja.

## Components

### Buttons

Secundarios de panel, texto marfil y borde de línea. Primarios dorados, texto oscuro y SemiBold. Padding compartido 16 horizontal y 10 vertical; variantes compactas mantienen su densidad local. Hover con borde dorado; foco marfil; desactivados a opacidad 0.45.

### Inputs / Fields

Campos oscuros de panel, texto marfil, cursor dorado, selección tonal y borde de línea. ComboBox con flecha vectorial y desplegable tonal. Campos de fecha oscuros y localizados (es-MX, dd/mm/aaaa), con calendario nativo e interacción de teclado.

### Navigation

Rail compacto con moneda, nombre, siete botones de icono y texto y versión al pie. Activo con fondo dorado y texto oscuro; inactivo transparente y texto marfil. El título de página es turquesa.

### Cards / Containers

Paneles tonales sin sombras, curvas de 10 unidades. Panel de lectura visual con padding de 22; ayuda de 18. Divisores separan bloques sin envolver cada cifra en una tarjeta.

### Tables and evidence

Tablas de solo lectura, filas compartidas de 46 unidades, alternancia fondo/panel, selección elevada y texto marfil. Encabezados azul gris con padding 12 horizontal y 11 vertical. Historial ordenable. Equipo reúne jugador, cofres, origen y cobertura; ≥ indica mínimo registrado y las estimaciones visuales quedan excluidas.

### Live reading

Acción dorada para activar y secundaria para detener. Estimación oculta al estar apagado, estado textual visible y diagnóstico plegado por defecto. Las filas de jugadores existentes incluyen nombre, origen y contexto de partida.

### Public player statistics

Estadísticas ocupa el segundo lugar del rail, bajo Resumen, y conserva paleta y controles WPF. La primera vista pide nombre Epic, PlayStation o Xbox; ofrece cuenta local detectada como opción, filtros y acción de consulta, sin campos de credenciales. Estado textual accesible, resultados con cuenta/contexto/fecha, seis métricas SemiBold de 32 unidades en columnas adaptables de 140 y tabla por modalidad con desplazamiento horizontal. Datos ausentes usan raya, no cero. Los errores explican una acción concreta. No usa la franja del lector de repeticiones; el proveedor y sus límites se registran en PRODUCT.md y el brief de superficie.

## Do's and Don'ts

### Do:
- Do mantener estado, procedencia y límites en texto.
- Do usar Segoe UI, controles nativos y diálogos del sistema.
- Do conservar la identidad pirata de moneda, cofre, rojo y dorado con acentos marinos.
- Do distinguir cuenta personal, equipo y estimación visual.

### Don't:
- Don't añadir transiciones ornamentales ni datos ficticios al producto.
- Don't presentar mínimos o estimaciones como totales exactos.
