/* ============ CONTACT SETTINGS — change these two lines to update everywhere ============ */
const WHATSAPP_NUMBER = '923702203020';      // country code + number, no + or spaces
const PHONE_DISPLAY   = '0370 2203020';      // how the number is shown on the site
const WA_BASE = 'https://wa.me/' + WHATSAPP_NUMBER;

// Email updates: each signup is emailed to the address below, and the subscriber gets an automatic welcome email.
// (Powered by FormSubmit.co — the FIRST signup sends you a one-time activation email; click its link once.)
const SUBSCRIBE_EMAIL    = 'kevibelion@gmail.com';

// The "Inspired by" collection is hidden until its real products, prices and photos are confirmed.
// Set to true only when those are ready (the pages must be restored too — ask for help).
const SHOW_INSPIRED = false;

// The "Fragrance notes" pyramid (Top / Heart / Base) on product pages stays hidden until the real, verified
// notes for each scent have been entered in PYRAMIDS (data.js). Set to true only after that.
const SHOW_FRAGRANCE_PYRAMID = false;

// PREVIEW MODE: while false, the site shows a "samples only" banner and ordering is switched off
// (buttons say "Coming soon"). Set to true on launch day, once real products, photos and prices are in.
const ORDERS_OPEN = false;
const SUBSCRIBE_ENDPOINT = 'https://formsubmit.co/ajax/' + SUBSCRIBE_EMAIL;

const SOCIAL = {
  instagram: 'https://instagram.com/kivabelion',
  tiktok:    'https://www.tiktok.com/@kivabelion',
  facebook:  'https://www.facebook.com/share/1DiZEfJ4zz/',
};

// Simple line/solid icons (drawn for this site) for the social links
const SOCIAL_ICONS = {
  instagram: '<svg viewBox="0 0 24 24" width="19" height="19" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3.5" y="3.5" width="17" height="17" rx="5"/><circle cx="12" cy="12" r="4"/><circle cx="17.2" cy="6.8" r="1" fill="currentColor" stroke="none"/></svg>',
  facebook:  '<svg viewBox="0 0 24 24" width="19" height="19" fill="currentColor" aria-hidden="true"><path d="M13.5 21v-7.6h2.6l.4-3.1h-3V8.4c0-.9.3-1.5 1.6-1.5h1.6V4.1c-.3 0-1.2-.1-2.3-.1-2.3 0-3.9 1.4-3.9 4v2.3H7.9v3.1h2.6V21h3z"/></svg>',
  tiktok:    '<svg viewBox="0 0 24 24" width="19" height="19" fill="currentColor" aria-hidden="true"><path d="M16.6 3h-2.9v11.6a2.6 2.6 0 1 1-2.6-2.6c.3 0 .5 0 .8.1V9.1a5.6 5.6 0 1 0 4.7 5.5V9a6.9 6.9 0 0 0 4 1.3V7.4A4.1 4.1 0 0 1 16.6 3z"/></svg>',
  whatsapp:  '<svg viewBox="0 0 32 32" width="19" height="19" fill="currentColor" aria-hidden="true"><path d="M16.02 3C9.4 3 4 8.4 4 15.02c0 2.36.66 4.56 1.8 6.44L4 29l7.72-1.75a11.9 11.9 0 0 0 4.3.8h.01c6.62 0 12.02-5.4 12.02-12.02C28.05 8.4 22.65 3 16.02 3zm5.4 18.57c-.25.7-1.45 1.34-2 1.42-.51.08-1.16.11-1.87-.12-.43-.14-.98-.32-1.7-.63-2.98-1.28-4.93-4.28-5.08-4.48-.15-.2-1.21-1.61-1.21-3.07s.77-2.17 1.04-2.47c.27-.3.59-.37.79-.37h.57c.18 0 .43-.07.67.51.25.6.85 2.07.92 2.22.07.15.12.32.02.52-.1.2-.15.32-.3.5-.15.17-.31.39-.45.52-.15.15-.3.31-.13.61.17.3.77 1.27 1.65 2.05 1.13 1.01 2.09 1.32 2.39 1.47.3.15.47.12.65-.07.17-.2.75-.87.95-1.17.2-.3.4-.25.67-.15.27.1 1.73.82 2.03.97.3.15.5.22.57.35.07.12.07.72-.18 1.42z"/></svg>',
};
function socialLinksHTML(){
  const items = [
    ['instagram','Instagram', SOCIAL.instagram],
    ['facebook','Facebook',  SOCIAL.facebook],
    ['tiktok','TikTok',      SOCIAL.tiktok],
    ['whatsapp','WhatsApp',  WA_BASE],
  ];
  return items.map(([k,label,url]) =>
    `<a href="${url}" target="_blank" rel="noopener" aria-label="${label}" title="${label}">${SOCIAL_ICONS[k]}</a>`
  ).join('');
}

/* ============ MEGA MENU CONTENT ============ */
/* href:null = no page/data exists yet, so the item shows as "Coming soon" instead of linking anywhere.
   Put the real page address in href when that page is ready. Product cards come from data.js (ids below). */
const MEGA_MENU = {
  shop: {
    label:'Shop', title:'Shop perfumes', width:940, featured:['noir','blanc','ocean'],
    promo:{eyebrow:'Not sure where to start?', title:'Find your scent', copy:"Three quick questions, and we'll point you to the Kiva Belion fragrance that fits you best.", label:'Take the quiz', href:'quiz.html'},
    links:[
      {label:'All Perfumes',       href:'signature-collection.html'},
      {label:"Men's Collection",   href:'signature-collection.html?for=men'},
      {label:"Women's Collection", href:'signature-collection.html?for=women'},
      {label:'Unisex Collection',  href:'signature-collection.html?for=unisex'},
      {label:'Best Sellers',       href:null},
      {label:'New Arrivals',       href:null},
    ],
    foot:{text:'Questions about our perfumes?', label:'Read the FAQs', href:'faq.html'}
  },
  collections: {
    label:'Collections', title:'Our collections', width:860, featured:['rouge','amber','vert'],
    promo:{eyebrow:'The collection', title:'Signature collection', copy:'Mixed and bottled by us, in Karachi.', label:'View full collection', href:'signature-collection.html'},
    links:[
      {label:'Signature Collection', href:'signature-collection.html'},
      ...(SHOW_INSPIRED ? [{label:'Inspired Collection', href:'inspired-collection.html'}] : []),
      {label:'Luxury Collection',    href:null},
      {label:'Oud Collection',       href:null},
      {label:'Gift Sets',            href:null},
    ],
    foot:{text:'Questions about our perfumes?', label:'Read the FAQs', href:'faq.html'}
  }
};
const MEGA_CHEVRON = '<svg class="chev" viewBox="0 0 12 12" width="10" height="10" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2.5 4.5 6 8l3.5-3.5"/></svg>';

function megaInnerHTML(key){
  const m = MEGA_MENU[key];
  const links = m.links.map((l,i) => l.href
    ? `<li style="--i:${i}"><a class="mega-link" href="${l.href}"><span>${l.label}</span></a></li>`
    : `<li style="--i:${i}"><span class="mega-link is-soon" aria-disabled="true"><span>${l.label}</span><small>Coming soon</small></span></li>`
  ).join('');
  return `<div class="mega-grid mega-${key}">
    <div class="mega-side">
      <p class="mega-eyebrow">${m.title}</p>
      <ul class="mega-links">${links}</ul>
    </div>
    <div class="mega-cards" data-mega-cards="${key}" data-count="${m.featured.length}"></div>
    <div class="mega-promo">
      <div class="mega-promo-text">
        <p class="mega-promo-eyebrow">${m.promo.eyebrow}</p>
        <p class="mega-promo-title">${m.promo.title}</p>
        <p class="mega-promo-copy">${m.promo.copy}</p>
      </div>
      <a class="mega-promo-cta" href="${m.promo.href}">${m.promo.label} <span aria-hidden="true">&rarr;</span></a>
    </div>
    <p class="mega-foot"><span>${m.foot.text}</span> <a href="${m.foot.href}">${m.foot.label} <span aria-hidden="true">&rarr;</span></a></p>
  </div>`;
}
function megaTriggerHTML(key){
  const m = MEGA_MENU[key];
  return `<div class="nav-item has-menu" data-key="${key}">
        <button type="button" class="nav-link nav-trigger" data-menu="${key}" aria-expanded="false" aria-haspopup="true">${m.label}${MEGA_CHEVRON}</button>
        <div class="nav-acc"><div class="nav-acc-inner"><button type="button" class="nav-back" aria-label="Back to main menu"><span class="nav-back-arrow" aria-hidden="true">&larr;</span> Back</button>${megaInnerHTML(key)}</div></div>
      </div>`;
}
function megaPanelHTML(key){
  return `<section class="mega-panel" data-panel="${key}" data-w="${MEGA_MENU[key].width}" aria-label="${MEGA_MENU[key].label}"><div class="mega-inner">${megaInnerHTML(key)}</div></section>`;
}

/* ============ SHARED HEADER / FOOTER / CART MARKUP ============ */
/* Injected at load time so every page stays in sync from one place. */

function injectHeader(){
  const el = document.getElementById('site-header');
  if(!el) return;
  el.outerHTML = `
${ORDERS_OPEN ? '' : `<div class="preview-bar" role="note">Preview — the products, photos and prices shown are samples. Orders open soon. <a href="${WA_BASE}?text=${encodeURIComponent('Hi Kiva Belion! Please let me know when you launch.')}" target="_blank" rel="noopener">Get notified on WhatsApp</a></div>`}
<header class="site-nav">
  <div class="nav-inner">
    <a href="index.html" class="logo-chip wordmark-link" aria-label="Kiva Belion home">
      <span class="wordmark"><span class="wm-a">KIVA</span> <span class="wm-b"><span class="wm-dot">BEL</span>ION</span></span>
    </a>
    <nav class="nav-links" id="navLinks" aria-label="Main">
      <span class="nav-pill" aria-hidden="true"></span>
      <span class="nav-indicator" aria-hidden="true"></span>
      <div class="nav-item"><a class="nav-link" data-nav="home" href="index.html">Home</a></div>
      ${megaTriggerHTML('shop')}
      ${megaTriggerHTML('collections')}
      <div class="nav-item"><a class="nav-link" data-nav="about" href="index.html#about">About Us</a></div>
      <div class="nav-item"><a class="nav-link" data-nav="contact" href="#contact">Contact</a></div>
    </nav>
    <div class="mega" id="mega" aria-hidden="true">
      <div class="mega-surface"><div class="mega-clip">
        ${megaPanelHTML('shop')}
        ${megaPanelHTML('collections')}
      </div></div>
    </div>
    <div class="nav-right">
      <button class="menu-toggle" id="menuToggle" aria-label="Toggle menu"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" aria-hidden="true"><path d="M4 7h16M4 12h16M4 17h16"/></svg></button>
      <button class="icon-btn" id="cartBtn" aria-label="Open bag">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 8h14l-1 12H6L5 8z"/><path d="M9 8V6a3 3 0 0 1 6 0v2"/></svg>
        <span class="cart-count" id="cartCount" style="display:none">0</span>
      </button>
    </div>
  </div>
</header>
`;
}

function injectFooterAndModals(){
  const el = document.getElementById('site-footer-modals');
  if(!el) return;
  el.outerHTML = `
<!-- EMAIL UPDATES -->
<section class="newsletter">
  <h3>Stay in the loop</h3>
  <p>New fragrances and restocks, straight to your inbox.</p>
  <form class="newsletter-form" id="newsletterForm" novalidate>
    <input type="email" required placeholder="Your email address" id="newsletterEmail" autocomplete="email" aria-label="Email address">
    <input type="text" name="_honey" id="newsletterHoney" tabindex="-1" autocomplete="off" style="position:absolute; left:-9999px; opacity:0; height:0; width:0;" aria-hidden="true">
    <button type="submit" class="btn btn-primary" id="newsletterBtn">Subscribe</button>
  </form>
  <div class="newsletter-msg" id="newsletterMsg" role="status" aria-live="polite"></div>
  <p class="newsletter-fine">We'll only use your email to send Kiva Belion updates. <a href="privacy.html">Privacy</a> · Prefer WhatsApp? <a id="updatesWaBtn" href="${WA_BASE}?text=${encodeURIComponent('Hi Kiva Belion! Please add me to your updates list.')}" target="_blank" rel="noopener">Join there instead</a>.</p>
</section>

<a class="whatsapp-float" href="${WA_BASE}" target="_blank" rel="noopener" aria-label="Chat on WhatsApp">
  <svg viewBox="0 0 32 32" fill="#fff"><path d="M16.02 3C9.4 3 4 8.4 4 15.02c0 2.36.66 4.56 1.8 6.44L4 29l7.72-1.75a11.9 11.9 0 0 0 4.3.8h.01c6.62 0 12.02-5.4 12.02-12.02C28.05 8.4 22.65 3 16.02 3zm0 21.9h-.01c-1.36 0-2.7-.36-3.87-1.04l-.28-.16-4.58 1.04 1.06-4.46-.18-.29a9.9 9.9 0 0 1-1.5-5.27C6.66 9.44 10.8 5.3 16.02 5.3c2.62 0 5.08 1.02 6.93 2.87a9.72 9.72 0 0 1 2.87 6.9c0 5.22-4.14 9.83-9.8 9.83zm5.4-7.33c-.3-.15-1.76-.87-2.03-.97-.27-.1-.47-.15-.67.15-.2.3-.77.97-.94 1.17-.17.2-.35.22-.65.07-.3-.15-1.25-.46-2.38-1.46-.88-.78-1.47-1.75-1.65-2.05-.17-.3-.02-.46.13-.61.13-.13.3-.35.45-.52.15-.17.2-.3.3-.5.1-.2.05-.37-.02-.52-.07-.15-.67-1.6-.92-2.2-.24-.57-.49-.5-.67-.5h-.57c-.2 0-.52.07-.79.37s-1.04 1.02-1.04 2.47 1.06 2.87 1.21 3.07c.15.2 2.1 3.2 5.08 4.48.71.3 1.26.49 1.7.63.71.22 1.36.19 1.87.12.57-.09 1.76-.72 2.01-1.42.25-.7.25-1.3.17-1.42-.07-.13-.27-.2-.57-.35z"/></svg>
</a>

<!-- FOOTER -->
<footer id="contact">
  <div class="footer-grid">
    <div class="footer-brand">
      <span class="logo-chip"><img src="assets/logo.png" alt="Kiva Belion" width="500" height="450" onerror="this.outerHTML='<span class=&quot;logo-text&quot;>KIVA BELION</span>'"></span>
      <p>Eau de parfums made by us in Karachi, Pakistan.</p>
      <div class="socials socials-brand">${socialLinksHTML()}</div>
      <div class="payment-note"><span class="dot"></span> Cash on Delivery &amp; WhatsApp orders</div>
    </div>
    <div>
      <h4>Shop</h4>
      <ul>
        <li><a href="signature-collection.html">Signature Collection</a></li>
        ${SHOW_INSPIRED ? '<li><a href="inspired-collection.html">Inspired by Icons</a></li>' : ''}
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
        <li><a href="privacy.html">Privacy Policy</a></li>
        <li><a href="terms.html">Terms of Service</a></li>
      </ul>
    </div>
    <div>
      <h4>Get in touch</h4>
      <ul>
        <li><a href="tel:+${WHATSAPP_NUMBER}">Call: ${PHONE_DISPLAY}</a></li>
        <li><a href="${WA_BASE}" target="_blank" rel="noopener">WhatsApp: ${PHONE_DISPLAY}</a></li>
        <li><a href="mailto:kevibelion@gmail.com">kevibelion@gmail.com</a></li>
        <li><span style="color:var(--text-muted); font-size:14px;">Karachi, Pakistan</span></li>
      </ul>
    </div>
  </div>
  <div class="footer-bottom">
    <span>© 2026 Kiva Belion. All rights reserved.</span>
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
injectFooterAndModals();
