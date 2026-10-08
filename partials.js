/* ============ CONTACT SETTINGS — change these two lines to update everywhere ============ */
const WHATSAPP_NUMBER = '923000000000';      // country code + number, no + or spaces (e.g. 923001234567)
const PHONE_DISPLAY   = '+92 300 0000000';   // how the number is shown on the site
const WA_BASE = 'https://wa.me/' + WHATSAPP_NUMBER;

/* ============ SHARED HEADER / FOOTER / CART MARKUP ============ */
/* Injected at load time so every page stays in sync from one place. */

function injectHeader(){
  const el = document.getElementById('site-header');
  if(!el) return;
  el.outerHTML = `
<header class="site-nav">
  <div class="nav-inner">
    <a href="index.html" class="logo-chip" aria-label="Kiva Belion home">
      <img src="assets/logo.png" alt="Kiva Belion">
    </a>
    <nav class="nav-links" id="navLinks">
      <a href="index.html">Home</a>
      <a href="signature-collection.html">Collection</a>
      <a href="inspired-collection.html">Inspired</a>
      <a href="quiz.html">Find Your Scent</a>
      <a href="index.html#about">About</a>
      <a href="faq.html">FAQs</a>
    </nav>
    <div class="nav-right">
      <button class="menu-toggle" id="menuToggle" aria-label="Toggle menu">☰</button>
      <button class="icon-btn" id="cartBtn" aria-label="Open cart">
        🛍
        <span class="cart-count" id="cartCount" style="display:none">0</span>
      </button>
    </div>
  </div>
</header>
`;
}

function injectTrustBar(){
  const el = document.getElementById('site-trustbar');
  if(!el) return;
  el.outerHTML = `
<section class="trust-bar">
  <div class="trust-bar-inner">
    <div class="trust-item"><span class="trust-icon">🇵🇰</span> Made in Pakistan</div>
    <div class="trust-item"><span class="trust-icon">🧴</span> Hand-bottled in small batches</div>
    <div class="trust-item"><span class="trust-icon">🚚</span> Cash on Delivery available</div>
    <div class="trust-item"><span class="trust-icon">↩</span> Easy 7-day returns</div>
  </div>
</section>
`;
}

function injectFooterAndModals(){
  const el = document.getElementById('site-footer-modals');
  if(!el) return;
  el.outerHTML = `
<!-- NEWSLETTER -->
<section class="newsletter">
  <h3>Stay in the loop</h3>
  <p>New launches, restocks, and small-batch drops — no spam, just scent.</p>
  <form class="newsletter-form" id="newsletterForm">
    <input type="email" required placeholder="Your email address" id="newsletterEmail">
    <button type="submit" class="btn btn-primary">Subscribe</button>
  </form>
  <div class="newsletter-msg" id="newsletterMsg">Thank you — you're on the list.</div>
</section>

<a class="whatsapp-float" href="${WA_BASE}" target="_blank" rel="noopener" aria-label="Chat on WhatsApp">
  <svg viewBox="0 0 32 32" fill="#fff"><path d="M16.02 3C9.4 3 4 8.4 4 15.02c0 2.36.66 4.56 1.8 6.44L4 29l7.72-1.75a11.9 11.9 0 0 0 4.3.8h.01c6.62 0 12.02-5.4 12.02-12.02C28.05 8.4 22.65 3 16.02 3zm0 21.9h-.01c-1.36 0-2.7-.36-3.87-1.04l-.28-.16-4.58 1.04 1.06-4.46-.18-.29a9.9 9.9 0 0 1-1.5-5.27C6.66 9.44 10.8 5.3 16.02 5.3c2.62 0 5.08 1.02 6.93 2.87a9.72 9.72 0 0 1 2.87 6.9c0 5.22-4.14 9.83-9.8 9.83zm5.4-7.33c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.17-.17.2-.35.22-.65.07-.3-.15-1.25-.46-2.38-1.46-.88-.78-1.47-1.75-1.65-2.05-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.07-.15-.67-1.6-.92-2.2-.24-.57-.49-.5-.67-.5h-.57c-.2 0-.52.07-.79.37s-1.04 1.02-1.04 2.47 1.06 2.87 1.21 3.07c.15.2 2.1 3.2 5.08 4.48.71.3 1.26.49 1.7.63.71.22 1.36.19 1.87.12.57-.09 1.76-.72 2.01-1.42.25-.7.25-1.3.17-1.42-.07-.13-.27-.2-.57-.35z"/></svg>
</a>

<!-- FOOTER -->
<footer id="contact">
  <div class="footer-grid">
    <div class="footer-brand">
      <span class="logo-chip"><img src="assets/logo.png" alt="Kiva Belion"></span>
      <p>Small-batch eau de parfums made and finished by hand in Karachi, Pakistan.</p>
      <div class="payment-note"><span class="dot"></span> Cash on Delivery &amp; WhatsApp orders</div>
    </div>
    <div>
      <h4>Shop</h4>
      <ul>
        <li><a href="signature-collection.html">Signature Collection</a></li>
        <li><a href="inspired-collection.html">Inspired by Icons</a></li>
        <li><a href="index.html#about">Our story</a></li>
        <li><a href="index.html">Home</a></li>
      </ul>
    </div>
    <div>
      <h4>Support</h4>
      <ul>
        <li><a href="returns.html">Shipping &amp; returns</a></li>
        <li><a href="quiz.html">Find your scent</a></li>
        <li><a href="faq.html">FAQs</a></li>
      </ul>
    </div>
    <div>
      <h4>Get in touch</h4>
      <ul>
        <li><a href="mailto:kevibelion@gmail.com">kevibelion@gmail.com</a></li>
        <li><a href="tel:+${WHATSAPP_NUMBER}">${PHONE_DISPLAY}</a></li>
        <li><span style="color:var(--text-muted); font-size:14px;">Karachi, Pakistan</span></li>
      </ul>
    </div>
  </div>
  <div class="footer-bottom">
    <span>© 2026 Kiva Belion. All rights reserved.</span>
    <div class="socials">
      <a href="#" aria-label="Instagram">IG</a>
      <a href="#" aria-label="Facebook">FB</a>
      <a href="#" aria-label="WhatsApp">WA</a>
    </div>
  </div>
</footer>

<!-- CART -->
<div class="overlay" id="overlay"></div>
<aside class="cart-drawer" id="cartDrawer" aria-label="Shopping cart">
  <div class="cart-head">
    <h3>Your bag</h3>
    <button class="close-btn" id="closeCart" aria-label="Close cart">&times;</button>
  </div>
  <div class="cart-items" id="cartItems"></div>
  <div class="cart-foot">
    <div class="subtotal-row"><span>Subtotal</span><strong id="cartSubtotal">PKR 0</strong></div>
    <button class="btn btn-primary" style="width:100%; justify-content:center;" id="checkoutBtn">Checkout</button>
    <button class="btn btn-whatsapp" style="width:100%; justify-content:center; margin-top:10px;" id="waOrderBtn">
      <svg viewBox="0 0 32 32" width="18" height="18" fill="#fff" aria-hidden="true"><path d="M16.02 3C9.4 3 4 8.4 4 15.02c0 2.36.66 4.56 1.8 6.44L4 29l7.72-1.75a11.9 11.9 0 0 0 4.3.8h.01c6.62 0 12.02-5.4 12.02-12.02C28.05 8.4 22.65 3 16.02 3zm5.4 18.57c-.25.7-1.45 1.34-2 1.42-.51.08-1.16.11-1.87-.12-.43-.14-.98-.32-1.7-.63-2.98-1.28-4.93-4.28-5.08-4.48-.15-.2-1.21-1.61-1.21-3.07s.77-2.17 1.04-2.47c.27-.3.59-.37.79-.37h.57c.18 0 .43-.07.67.51.25.6.85 2.07.92 2.22.07.15.12.32.02.52-.1.2-.15.32-.3.5-.15.17-.31.39-.45.52-.15.15-.3.31-.13.61.17.3.77 1.27 1.65 2.05 1.13 1.01 2.09 1.32 2.39 1.47.3.15.47.12.65-.07.17-.2.75-.87.95-1.17.2-.3.4-.25.67-.15.27.1 1.73.82 2.03.97.3.15.5.22.57.35.07.12.07.72-.18 1.42z"/></svg>
      Order via WhatsApp
    </button>
  </div>
</aside>

<!-- CHECKOUT MODAL -->
<div class="modal" id="checkoutModal">
  <div class="modal-box" id="modalBox"></div>
</div>
`;
}

injectHeader();
injectTrustBar();
injectFooterAndModals();
