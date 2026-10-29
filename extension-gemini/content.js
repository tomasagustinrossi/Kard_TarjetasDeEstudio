// Guardar para Tarjetas — funciona 100 % en local.
// No hace ninguna petición de red, no usa permisos especiales y solo corre en gemini.google.com.
// Hace una sola cosa: toma el texto que seleccionaste (o la última respuesta de Gemini)
// y lo descarga como archivo .txt en tu carpeta de Descargas.
(() => {
  const ID_BOTON = "tarjetas-guardar";
  if (document.getElementById(ID_BOTON)) return;

  function textoAGuardar() {
    const seleccion = window.getSelection()?.toString().trim();
    if (seleccion) return seleccion;

    // Si no hay selección, se intenta con la última respuesta de Gemini (puede cambiar si Gemini cambia su página).
    const respuestas = document.querySelectorAll(
      "message-content, .model-response-text, [data-message-author-role='model']"
    );
    return respuestas.length ? respuestas[respuestas.length - 1].innerText.trim() : "";
  }

  function descargar(texto) {
    const fecha = new Date().toISOString().slice(0, 10);
    const blob = new Blob([texto], { type: "text/plain;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement("a");
    enlace.href = url;
    enlace.download = `tarjetas-gemini-${fecha}.txt`;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  function aviso(mensaje) {
    const caja = document.createElement("div");
    caja.textContent = mensaje;
    Object.assign(caja.style, {
      position: "fixed", right: "20px", bottom: "76px", zIndex: 2147483647,
      padding: "10px 14px", borderRadius: "10px", background: "#202124", color: "#fff",
      font: "14px system-ui, sans-serif", boxShadow: "0 4px 14px rgba(0,0,0,.3)"
    });
    document.body.appendChild(caja);
    setTimeout(() => caja.remove(), 3500);
  }

  const boton = document.createElement("button");
  boton.id = ID_BOTON;
  boton.textContent = "Guardar para Tarjetas";
  Object.assign(boton.style, {
    position: "fixed", right: "20px", bottom: "20px", zIndex: 2147483647,
    padding: "10px 16px", border: "none", borderRadius: "22px", cursor: "pointer",
    background: "#FBBC04", color: "#202124", font: "bold 14px system-ui, sans-serif",
    boxShadow: "0 4px 14px rgba(0,0,0,.3)"
  });

  // Evita que al hacer clic se pierda la selección de texto.
  boton.addEventListener("mousedown", (e) => e.preventDefault());
  boton.addEventListener("click", () => {
    const texto = textoAGuardar();
    if (!texto) {
      aviso("Seleccioná primero el texto con tus tarjetas.");
      return;
    }
    descargar(texto);
    aviso("Listo: se guardó en Descargas. Importalo desde la app Tarjetas.");
  });

  document.body.appendChild(boton);
})();
