(function () {
  const tarjetas = document.querySelectorAll('.monitor-card[data-reserva]');
  if (!tarjetas.length || typeof signalR === 'undefined') return;

  const conexion = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/monitoreo')
    .withAutomaticReconnect()
    .build();

  conexion.on('metricaActualizada', (m) => {
    const tarjeta = document.querySelector(`.monitor-card[data-reserva="${m.idReserva}"]`);
    if (!tarjeta) return;
    tarjeta.querySelector('.js-actividad').textContent = m.nivelActividad + '%';
    tarjeta.querySelector('.js-temp').textContent = m.temperaturaC + '°C';
    tarjeta.querySelector('.js-comida').textContent = m.ultimaComida;
    tarjeta.querySelector('.js-hora').textContent = m.timestamp;
  });

  conexion.start()
    .then(() => {
      tarjetas.forEach(t => conexion.invoke('UnirseAReserva', parseInt(t.dataset.reserva, 10)));
    })
    .catch(err => console.error('No se pudo conectar al monitoreo en vivo:', err));
})();
