# Parcheador Ace Attorney Trilogy (2019)

## Introducción
Parcheador de Ace Attorney Trilogy (2019) al español. El idioma se inserta como uno más del juego, permitiendo conservar las partidas guardadas de los otros idiomas.

*SI  ESTÁS AQUÍ Y SOLO QUIERES PARCHEAR EL JUEGO, VE A LA [PESTAÑA DE RELEASES](https://github.com/CTPache/ParcheadorAAT/releases/latest), AUNQUE SE RECOMIENDA LA LECTURA DEL SIGUIENTE DOCUMENTO, SOBRE TODO LA PARTE MARCADA COMO IMPORTANTE Y LAS INSTRUCCIONES.*

![imagen](https://user-images.githubusercontent.com/25833407/146673825-467c390c-1139-4958-a696-52602c2a3e2b.png)

### IMPORTANTE
- La biblioteca/librería que reproduce sonidos en el parcheador es exclusiva a Windows, así que por eso el parcheador de Linux no tiene sonido.
- Si utilizas Windows 7, se requiere el Service Pack 1 y la actualización KB3063858.
- Una vez el parche esté aplicado, tienes que ir a los ajustes y cambiar el idioma desde ahí (será el último de la lista).
- En el caso de querer mover la instalación del juego, se recomienda primero moverla con la herramienta de Steam, y a continuación comprobar que no haya quedado nada en la ubicación anterior. Si queda algo, se puede cortar y pegar.
- Se recomienda hacer lo mismo si se quiere desinstalar el juego, primero desinstalarlo desde Steam y a continuación eliminar el directorio del juego de forma manual.

## Instrucciones
1. Este paso dependerá del sistema operativo que uses:
    - Si utilizas **Windows**, [descarga "AATrilogyPatcher.exe" de la última release.](https://github.com/Traducciones-Kurain/AATrilogy-2019-ESP/releases/latest/download/AATrilogyPatcher.exe)
    - Si utilizas **Linux**, [descarga "AATrilogyPatcher-linux" de la última release.](https://github.com/Traducciones-Kurain/AATrilogy-2019-ESP/releases/latest/download/AATrilogyPatcher-linux)<br/>
_Si utilizas una **Steam Deck**, la opción que debes usar es la de Linux._
2. Abrir el archivo descargado.
3. Presionar el botón "Parchear".
4. Se abrirá una ventana que mostrara donde se va a aplicar el parche. Este directorio no puede ser modificado, ya que este proviene de donde Steam dice que ha instalado el juego.
5. Darle al botón "Sí" y esperar a que se descargue y aplique el parche.
    - Si se requiere, es posible descargar el parche por separado ([desde la última release](https://github.com/Traducciones-Kurain/AATrilogy-2019-ESP/releases/latest/download/Patch-Steam.zip)) y colocarlo en la misma ubicación que el parcheador. De esta manera se saltará la descarga y se aplicará directamente.
6. Una vez aplicado, abrir el juego normalmente desde Steam. Si el juego aparece en otro idioma, cambiar a español en el menú de opciones.

## Troubleshooting

### Es posible que el parcheador no encuentre el directorio del juego.
Eso puede deberse a estos motivos:
  - El juego no está instalado.
  - El juego no fue instalado mediante Steam.
  - El juego se instaló y se movió de forma manual (cortar y pegar).

En cualquiera de los tres casos, la solución más sencilla es (re)instalar el juego desde Steam.

### Es posible que el parcheador falle.
Puede suceder que el parcheador no sea capaz de escribir en el directorio donde se encuentra. Esto se puede solucionar de dos maneras:

- Moviendo el ejecutable a un directorio donde tenga permisos, como puede ser el escritorio o la carpeta de Documentos.
- Ejecutándolo como administrador (no recomendado, hacer eso solo si lo anterior no funcionó).

### [Ubuntu] "No usable version of libssl was found" al ejecutar el parcheador
Si te sale este error, y utilizas **Ubuntu**, debes instalar **libssl1.0-dev**. Para hacerlo, simplemente pon estos tres comandos en la terminal:
```
sudo add-apt-repository ppa:rael-gc/rvm
sudo apt-get update
sudo apt install libssl1.0-dev
```
_Fuente: https://stackoverflow.com/a/73066454_

Si necesitas soporte más allá de lo que hay aquí escrito, escribe en el canal de ``#soporte-parches`` de nuestro [servidor de Discord](https://discord.gg/dtaFZcWmUA).

# Créditos
## Créditos del parche
- **Dirección:** [JauCR](https://github.com/JauCR/)<br/>
- **Edición de gráficos:** [JauCR](https://github.com/JauCR/), MF5K<br/>
- **Romhacking:** [CTPache](https://github.com/CTPache), [Worst Aqua Player](https://github.com/WorstAquaPlayer), Kaplas, Dant (del proyecto [Судебный поворот](https://gamecom.neocities.org/Ace_Attorney/Translations/Sudebnyy_povorot_Trilogiya_Steam/))<br/>
- **Testeo:** [CTPache](https://github.com/CTPache), franciskirk, [Worst Aqua Player](https://github.com/WorstAquaPlayer)<br/>
- **Agradecimientos especiales:** Retroductor, roli 300, LegendaryX77, Dj_Mike238 (del grupo DragonPunk Team), el grupo [Jacutem Sabão](https://jacutemsabao.bitbucket.io), a [TraduSquare](https://tradusquare.es) y a CAPCOM por la traducción original de DS.

## Créditos del parcheador
- Interfaz escrita por [Worst Aqua Player](https://github.com/WorstAquaPlayer), utilizando [Avalonia](https://github.com/AvaloniaUI/Avalonia) y las bibliotecas/librerías [Avalonia.GIF](https://github.com/AvaloniaUI/Avalonia.GIF) y [NAudio](https://github.com/naudio/NAudio).

- Correcciones de texto de la interfaz por [CTPache](https://github.com/CTPache).

- Parcheador escrito por [Worst Aqua Player](https://github.com/WorstAquaPlayer), utilizando la biblioteca/librería [Mara](https://github.com/TraduSquare/Mara).

- Agradecimientos a [Darkmet98](https://github.com/Darkmet98) por el [parcheador de Disgaea](https://github.com/Darkmet98/DisgaeaPatcher) y el [parcheador de Okami](https://github.com/Darkmet98/OkamiPatcher).

## Créditos del parcheador anterior
- Parcheador escrito por [CTPache](https://github.com/CTPache).

- Interfaz escrita por [CTPache](https://github.com/CTPache) y [legendaryX77](https://github.com/legendaryX77).
