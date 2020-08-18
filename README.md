# WarField (explicación técnica)

El proyecto ha sido realizado con el motor de videojuegos Unity3Dy desarrollado en el lenguaje C# (único disponible a día de hoy para este motor). El proyecto consiste en un juego multijugador en el que se utiliza un componente propietario llamado Network Manager proporcionado por Unity para conectar a varios jugadores en una misma partida. Dentro de una partida los jugadores podrán comunicarse entre ellos mediante un sistema diseñado propiamente para este fin en el cual de momento solo puede albergar una comunicación punto a punto en 2 usuarios, es decir, en una misma partida podría existir 4 jugadores (hasta un máximo de 10), pero solamente se podrán comunicar entre ellos 2 y 2, pudiendo cambiar esto en cualquier momento.

La comunicación entre los jugadores se realiza empleando una API gratuita (en el momento en el que se desarrolló esta aplicación) la cual mediante una conexión utilizando el framework WebRTC los jugadores pudieran comunicarse a través de mensajes de texto. El objetivo primordial de este proyecto ha sido modificar esta API para que los jugadores además de comunicarse a través de un chat con solo texto, pudieran también hacerlo en mediante una videoconferencia, utilizando para ello el protocolo UDP. Empleando este protocolo de transporte la comunicación entre los usuarios se realiza de forma rápida y sin acuso de recibo, aunque eso no impide que se tuviera que ajustar la frecuencia de muestreo y limitar la calidad de imagen impuestas por las cámaras. Se debe recalcar también, que la compresión de la imagen y el sonido ha sido realizada manualmente en unos de los scripts desarrollados en esta aplicación, por lo que en los scripts del mismo se podrán ver la mayoria de los pasos detallados.

El juego es multiplataforma, por lo que se podrá jugar tanto en PC, Android y en las Oculus Go (la versión de las Oculus Go se encuentra en otro repositorio por cuestiones de tamaño).



 SCRIPTS: EN LA CARPTETA ASSETS/SCRIPTS se pueden encontrar todos los scripts del juego que se ha desarrollado.
