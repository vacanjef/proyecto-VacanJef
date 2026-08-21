// ── Partículas flotantes de fondo ──────────────────────────
(function () {
  const c = document.getElementById('pawsBg');
  if (!c) return;
  const p = ['🐾', '🐕', '🦮', '🌟', '☀️', '🐩'];
  for (let i = 0; i < 22; i++) {
    const el = document.createElement('div');
    el.className = 'paw';
    el.textContent = p[i % p.length];
    el.style.cssText = `left:${Math.random() * 100}vw;top:${Math.random() * 100}vh;font-size:${1 + Math.random() * 2.2}rem;--dur:${10 + Math.random() * 12}s;--delay:${Math.random() * 10}s`;
    c.appendChild(el);
  }
})();

// ── Carrusel del panel de marca ─────────────────────────────
(function () {
  const track = document.getElementById('carouselTrack');
  if (!track) return;
  const dotsContainer = document.getElementById('carouselDots');
  const slides = track.querySelectorAll('.carousel-slide');
  const total = slides.length;
  let current = 0, timer;

  slides.forEach((_, i) => {
    const dot = document.createElement('button');
    dot.className = 'dot' + (i === 0 ? ' active' : '');
    dot.setAttribute('aria-label', 'Imagen ' + (i + 1));
    dot.onclick = () => goTo(i);
    dotsContainer.appendChild(dot);
  });

  function goTo(n) {
    current = (n + total) % total;
    track.style.transform = `translateX(-${current * 100}%)`;
    document.querySelectorAll('.dot').forEach((d, i) => d.classList.toggle('active', i === current));
  }
  function next() { goTo(current + 1); }
  function startTimer() { timer = setInterval(next, 4000); }
  function stopTimer() { clearInterval(timer); }

  startTimer();
  const carousel = document.getElementById('carousel');
  carousel.addEventListener('mouseenter', stopTimer);
  carousel.addEventListener('mouseleave', startTimer);
})();

// ── Mostrar/ocultar contraseña ──────────────────────────────
function togglePassword() {
  const input = document.getElementById('Password');
  const btn = document.getElementById('togglePass');
  const visible = input.type === 'text';
  input.type = visible ? 'password' : 'text';
  btn.textContent = visible ? '👁️' : '🙈';
}

// ── Autocompletar credenciales de demostración ──────────────
function fillDemo(email, password) {
  document.getElementById('Correo').value = email;
  document.getElementById('Password').value = password;
}
