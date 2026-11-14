/* ============ SHARED APP BEHAVIOR (nav, cart, checkout, effects) ============ */

// carousel arrow scrolling (home page)
document.querySelectorAll('.carousel-arrow').forEach(btn=>{
  btn.addEventListener('click', ()=>{
    const track = document.getElementById(btn.dataset.target);
    if(!track) return;
    const dir = btn.classList.contains('left') ? -1 : 1;
    track.scrollBy({left: dir * 270, behavior:'smooth'});
  });
});

// ---------- CART STATE ----------
// The bag is kept in this browser's localStorage so it survives moving between pages (every page of this
// site is a separate page load). Only product ids and quantities are saved. Names and prices are always
// read fresh from data.js, and customer details (name, phone, address) are never saved.
const CART_STORAGE_KEY = 'kivabelion.cart.v1';
const CART_MAX_QTY = 99;   // sanity guard against corrupt data, not a shop rule
let cart = [];

// Browsers can block or fill storage (private mode, site settings), so every access is guarded.
// If storage does not work, the bag still works in memory for the current page.
const cartStorageOk = (function probeCartStorage(){
  try{
    const k = '__kb_probe__';
    window.localStorage.setItem(k, '1');
    window.localStorage.removeItem(k);
    return true;
  } catch(e){ return false; }
})();

// Returns the saved bag as full product objects, or null when storage cannot be read
// (in that case the caller keeps the bag it already has in memory).
function readStoredCart(){
  if(!cartStorageOk) return null;
  let raw;
  try{ raw = window.localStorage.getItem(CART_STORAGE_KEY); } catch(e){ return null; }
  if(!raw) return [];
  let data;
  try{ data = JSON.parse(raw); } catch(e){ return []; }
  if(!data || data.v !== 1 || !Array.isArray(data.items)) return [];
  const items = [];
  data.items.forEach(entry=>{
    if(!entry || typeof entry.id !== 'string') return;
    const p = getProductById(entry.id);                 // only known, orderable products are accepted
    if(!p || orderState(p) !== 'ok') return;
    if(items.some(i=>i.id===p.id)) return;              // ignore duplicates
    const qty = Math.floor(Number(entry.qty));
    if(!Number.isFinite(qty) || qty < 1) return;
    items.push({...p, qty: Math.min(qty, CART_MAX_QTY)});
  });
  return items;
}
function saveCart(){
  if(!cartStorageOk) return;
  try{
    if(cart.length === 0){
      window.localStorage.removeItem(CART_STORAGE_KEY);
    } else {
      window.localStorage.setItem(CART_STORAGE_KEY, JSON.stringify({v:1, items: cart.map(i=>({id:i.id, qty:i.qty}))}));
    }
  } catch(e){ /* storage full or blocked: keep working in memory */ }
}
// Re-read the saved bag so a change made in another tab is never overwritten by an out-of-date copy.
function syncCartFromStorage(){
  const stored = readStoredCart();
  if(stored) cart = stored;
}

function addToCart(id, qty){
  qty = Math.floor(Number(qty)) || 1;
  if(qty < 1) qty = 1;
  syncCartFromStorage();
  const existing = cart.find(i=>i.id===id);
  if(existing){ existing.qty = Math.min(existing.qty + qty, CART_MAX_QTY); } else {
    const p = getProductById(id);
    if(!p || orderState(p) !== 'ok') return;
    cart.push({...p, qty: Math.min(qty, CART_MAX_QTY)});
  }
  saveCart();
  renderCart();
}
function changeQty(id, delta){
  syncCartFromStorage();
  const item = cart.find(i=>i.id===id);
  if(!item){ renderCart(); return; }
  item.qty = Math.min(item.qty + delta, CART_MAX_QTY);
  if(item.qty<=0){ cart = cart.filter(i=>i.id!==id); }
  saveCart();
  renderCart();
}
function removeItem(id){
  syncCartFromStorage();
  cart = cart.filter(i=>i.id!==id);
  saveCart();
  renderCart();
}
function cartTotal(){
  return cart.reduce((sum,i)=>sum+i.price*i.qty,0);
}
function renderCart(){
  const wrap = document.getElementById('cartItems');
  const countEl = document.getElementById('cartCount');
  if(!wrap || !countEl) return;
  const totalQty = cart.reduce((s,i)=>s+i.qty,0);
  if(totalQty>0){ countEl.style.display='flex'; countEl.textContent = totalQty; }
  else { countEl.style.display='none'; }
  const bagBtn = document.getElementById('cartBtn');
  if(bagBtn) bagBtn.setAttribute('aria-label', totalQty>0 ? 'Open bag, ' + totalQty + (totalQty===1 ? ' item' : ' items') : 'Open bag');

  if(cart.length===0){
    wrap.innerHTML = `<div class="cart-empty">Your bag is empty.<br>Add a fragrance to get started.</div>`;
  } else {
    wrap.innerHTML = cart.map(i=>`
      <div class="cart-item">
        <div class="bwrap ${i.image ? 'photo-wrap' : 'liquid-'+i.id}">${i.image ? `<img src="${i.image}" alt="${i.name}">` : `<svg viewBox="0 0 120 240"><use href="#bottle-shape"/></svg>`}</div>
        <div class="cart-item-info">
          <h5>${i.name}</h5>
          <div class="p">PKR ${(i.price*i.qty).toLocaleString()}</div>
          <div class="qty-row">
            <button class="qty-btn" aria-label="Decrease quantity of ${i.name}" onclick="changeQty('${i.id}',-1)">−</button>
            <span>${i.qty}</span>
            <button class="qty-btn" aria-label="Increase quantity of ${i.name}" onclick="changeQty('${i.id}',1)">+</button>
            <button type="button" class="remove-link" aria-label="Remove ${i.name} from bag" onclick="removeItem('${i.id}')">Remove</button>
          </div>
        </div>
      </div>
    `).join('');
  }
  const subtotalEl = document.getElementById('cartSubtotal');
  if(subtotalEl) subtotalEl.textContent = 'PKR ' + cartTotal().toLocaleString();
  // Checkout and WhatsApp only work when the bag holds at least one valid item
  const hasItems = cart.some(i => i && i.qty > 0 && i.price > 0);
  ['checkoutBtn'].forEach(id=>{
    const b = document.getElementById(id); if(!b) return;
    b.disabled = !hasItems;
    b.setAttribute('aria-disabled', hasItems ? 'false' : 'true');
  });
}

function handleAddClick(e){
  if(e.target.classList.contains('add-btn')){
    e.preventDefault();
    const id = e.target.dataset.id;
    addToCart(id);
    e.target.textContent = 'Added ✓';
    e.target.classList.add('added');
    setTimeout(()=>{ e.target.textContent='Add to bag'; e.target.classList.remove('added'); }, 1400);
  }
}
document.body.addEventListener('click', handleAddClick);

// ---------- DRAWER TOGGLES ----------
const overlay = document.getElementById('overlay');
const cartDrawer = document.getElementById('cartDrawer');
function openCart(){ if(cartDrawer){ cartDrawer.classList.add('show'); overlay.classList.add('show'); } }
function closeCartFn(){ if(cartDrawer){ cartDrawer.classList.remove('show'); overlay.classList.remove('show'); } }
const cartBtnEl = document.getElementById('cartBtn');
if(cartBtnEl) cartBtnEl.addEventListener('click', openCart);
const closeCartEl = document.getElementById('closeCart');
if(closeCartEl) closeCartEl.addEventListener('click', closeCartFn);
if(overlay) overlay.addEventListener('click', ()=>{ closeCartFn(); closeCheckout(); });

// mobile menu
const navLinks = document.getElementById('navLinks');
const menuToggleEl = document.getElementById('menuToggle');
if(menuToggleEl && navLinks){
  menuToggleEl.addEventListener('click', ()=>{ navLinks.classList.toggle('show'); });
  navLinks.querySelectorAll('a').forEach(a=>a.addEventListener('click', ()=>navLinks.classList.remove('show')));
}

// ---------- CHECKOUT (Cash on Delivery) ----------
// The order is sent to the Kiva Belion API, which checks every price and total again on the server and
// saves the order. The bag is only cleared, and a confirmation only shown, after the server answers that the
// order was really created. No card or bank details are ever asked for. Nothing personal is stored in the browser.
const checkoutModal = document.getElementById('checkoutModal');
const modalBox = document.getElementById('modalBox');

const CHECKOUT_PHONE_ALLOWED = /^\+?[0-9 ()\-]{7,30}$/;
const CHECKOUT_PHONE_COMPACT = /^(?:\+?92|0)?[0-9]{9,10}$/;
const CHECKOUT_EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
const CHECKOUT_TIMEOUT_MS = 20000;
let checkoutBusy = false;          // true while an order is being sent: blocks double submits
let checkoutAttempt = null;        // {sig, key}: the same key is reused when the SAME order is retried

function escHTML(v){
  return String(v).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function compactPhone(v){
  return String(v).trim().split('').filter((c,i)=> /\d/.test(c) || (c==='+' && i===0)).join('');
}
function newIdempotencyKey(){
  try{ if(window.crypto && crypto.randomUUID) return crypto.randomUUID(); }catch(e){}
  const a = new Uint8Array(16);
  try{ crypto.getRandomValues(a); }catch(e){ for(let i=0;i<16;i++) a[i] = Math.floor(Math.random()*256); }
  return Array.prototype.map.call(a, b => ('0'+b.toString(16)).slice(-2)).join('');
}
function attemptKeyFor(signature){
  if(!checkoutAttempt || checkoutAttempt.sig !== signature) checkoutAttempt = {sig: signature, key: newIdempotencyKey()};
  return checkoutAttempt.key;
}

function validateCheckout(v){
  const e = {};
  if(v.name.length < 2 || v.name.length > 150 || !/[A-Za-zÀ-ɏ؀-ۿ]/.test(v.name)) e.fname = 'Please enter your full name.';
  if(!CHECKOUT_PHONE_ALLOWED.test(v.phone) || !CHECKOUT_PHONE_COMPACT.test(compactPhone(v.phone))) e.fphone = 'Please enter a valid phone number, for example 0300 1234567.';
  if(!v.email || v.email.length > 254 || !CHECKOUT_EMAIL.test(v.email)) e.femail = 'Please enter a valid email address.';
  if(v.address.length < 10 || v.address.length > 200) e.faddress = 'Please enter your complete delivery address (house, street, area).';
  if(v.city.length < 2 || v.city.length > 100) e.fcity = 'Please enter your city.';
  if(v.giftMessage.length > 200) e.fgiftmsg = 'The gift message can be at most 200 characters.';
  return e;
}
// Server error keys -> form field ids
const CHECKOUT_FIELD_MAP = {
  'customer.fullName':'fname', 'customer.phone':'fphone', 'customer.email':'femail',
  'shipping.line1':'faddress', 'shipping.city':'fcity', 'note':'fgiftmsg'
};

function checkoutFormHTML(){
  const rows = cart.map(i => `
      <li><span class="cs-name">${escHTML(i.name)} <span class="cs-qty">× ${i.qty}</span></span><span class="cs-amt">PKR ${(i.price*i.qty).toLocaleString()}</span></li>`).join('');
  const field = (id, label, input, extra) => `
      <div class="field">
        <label for="${id}">${label}</label>
        ${input}
        <div class="field-error" id="${id}Err" role="alert"></div>
        ${extra || ''}
      </div>`;
  return `
    <button type="button" class="modal-close-btn" id="checkoutCloseBtn" aria-label="Close checkout">&times;</button>
    <h2 id="checkoutTitle">Checkout</h2>
    <div class="sub">Cash on Delivery · pay when your order arrives</div>
    <form id="checkoutForm" novalidate>
      <div class="order-summary" aria-label="Order summary">
        <ul class="cs-list">${rows}</ul>
        <div class="cs-total"><span>Total</span><strong>PKR ${cartTotal().toLocaleString()}</strong></div>
      </div>
      ${field('fname','Full name','<input type="text" id="fname" autocomplete="name" maxlength="150" required>')}
      ${field('fphone','Phone number','<input type="tel" id="fphone" autocomplete="tel" inputmode="tel" maxlength="30" required placeholder="03XX XXXXXXX">')}
      ${field('femail','Email address','<input type="email" id="femail" autocomplete="email" maxlength="254" required placeholder="you@example.com">')}
      ${field('faddress','Complete delivery address','<input type="text" id="faddress" autocomplete="street-address" maxlength="200" required placeholder="House / flat, street, area">')}
      ${field('fcity','City','<input type="text" id="fcity" autocomplete="address-level2" maxlength="100" required placeholder="Karachi">')}
      <div class="field gift-field">
        <label class="gift-check"><input type="checkbox" id="fgift"> This order is a gift</label>
        <textarea id="fgiftmsg" rows="2" maxlength="200" placeholder="Message for the recipient (optional)" hidden></textarea>
        <div class="field-error" id="fgiftmsgErr" role="alert"></div>
      </div>
      <div class="field">
        <label>Payment method</label>
        <div class="pay-options">
          <div class="pay-option active" data-method="cod">Cash on Delivery</div>
        </div>
        <p class="pay-note">Your order is <strong>unpaid until it is delivered</strong>. Please keep the exact amount in cash ready.</p>
      </div>
      <div class="checkout-status" id="checkoutStatus" role="alert" aria-live="assertive"></div>
      <button type="submit" class="btn btn-primary modal-submit" id="placeOrderBtn">Place order · Cash on Delivery</button>
    </form>
  `;
}

function setCheckoutStatus(text, kind){
  const el = modalBox.querySelector('#checkoutStatus'); if(!el) return;
  el.textContent = text || '';
  el.className = 'checkout-status' + (text ? ' show ' + (kind || 'error') : '');
}
function showFieldErrors(errors){
  ['fname','fphone','femail','faddress','fcity','fgiftmsg'].forEach(id=>{
    const input = modalBox.querySelector('#'+id), err = modalBox.querySelector('#'+id+'Err');
    if(!input || !err) return;
    const msg = errors[id] || '';
    err.textContent = msg;
    if(msg){ input.setAttribute('aria-invalid','true'); input.setAttribute('aria-describedby', id+'Err'); }
    else { input.removeAttribute('aria-invalid'); input.removeAttribute('aria-describedby'); }
  });
  const first = ['fname','fphone','femail','faddress','fcity','fgiftmsg'].find(id => errors[id]);
  if(first){ const el = modalBox.querySelector('#'+first); if(el) el.focus(); }
}
function setCheckoutBusy(busy){
  checkoutBusy = busy;
  const btn = modalBox.querySelector('#placeOrderBtn');
  if(btn){ btn.disabled = busy; btn.textContent = busy ? 'Placing your order…' : 'Place order · Cash on Delivery'; }
  const form = modalBox.querySelector('#checkoutForm'); if(form) form.setAttribute('aria-busy', busy ? 'true' : 'false');
}
// A response only counts as "order saved" when it has the shape of a real order from the server.
function isSavedOrder(d){
  return !!d && typeof d === 'object' && typeof d.orderNumber === 'string' && /^[A-Z0-9-]{6,30}$/.test(d.orderNumber)
    && typeof d.total === 'number' && Array.isArray(d.items) && d.items.length > 0;
}

function showOrderConfirmation(order, city){
  const rows = order.items.map(i => `<li><span class="cs-name">${escHTML(i.name)} <span class="cs-qty">× ${Number(i.quantity)}</span></span><span class="cs-amt">PKR ${Number(i.lineTotal).toLocaleString()}</span></li>`).join('');
  const fee = Number(order.shippingFee) > 0 ? `<div class="cs-row"><span>Delivery</span><span>PKR ${Number(order.shippingFee).toLocaleString()}</span></div>` : '';
  modalBox.innerHTML = `
    <button type="button" class="modal-close-btn" id="checkoutCloseBtn2" aria-label="Close">&times;</button>
    <div class="order-confirm">
      <div class="check-circle" aria-hidden="true">✓</div>
      <h2 id="checkoutTitle">Order received</h2>
      <p>Thank you${city ? '' : ''}! Your order has been placed.</p>
      <p class="order-ref-label">Your order reference</p>
      <p class="order-ref" id="orderRef">${escHTML(order.orderNumber)}</p>
      <div class="order-summary">
        <ul class="cs-list">${rows}</ul>
        ${fee}
        <div class="cs-total"><span>Total</span><strong>PKR ${Number(order.total).toLocaleString()}</strong></div>
      </div>
      <p><strong>Cash on Delivery</strong> — unpaid until delivery. Please keep <strong>PKR ${Number(order.total).toLocaleString()}</strong> in cash ready.</p>
      <p style="margin-top:8px;">Please save your order reference in case you need to contact us.</p>
      <div><button class="btn btn-primary" style="margin-top:16px;" id="closeConfirm">Continue shopping</button></div>
    </div>
  `;
  modalBox.querySelector('#closeConfirm').addEventListener('click', closeCheckout);
  modalBox.querySelector('#checkoutCloseBtn2').addEventListener('click', closeCheckout);
  const ref = modalBox.querySelector('#orderRef'); if(ref){ ref.setAttribute('tabindex','-1'); ref.focus(); }
}

async function submitOrder(){
  if(checkoutBusy) return;                                   // duplicate submission guard
  syncCartFromStorage();
  if(cart.length === 0){ closeCheckout(); renderCart(); return; }

  const v = {
    name: modalBox.querySelector('#fname').value.trim(),
    phone: modalBox.querySelector('#fphone').value.trim(),
    email: modalBox.querySelector('#femail').value.trim(),
    address: modalBox.querySelector('#faddress').value.trim(),
    city: modalBox.querySelector('#fcity').value.trim(),
    gift: modalBox.querySelector('#fgift').checked,
    giftMessage: modalBox.querySelector('#fgiftmsg').value.trim(),
  };
  const errors = validateCheckout(v);
  showFieldErrors(errors);
  if(Object.keys(errors).length){ setCheckoutStatus('Please check the highlighted fields.', 'error'); return; }
  setCheckoutStatus('');

  if(!ORDERS_API_BASE){
    setCheckoutStatus('Online ordering is not available right now, so your order has NOT been placed. Please try again later.', 'error');
    return;
  }

  const body = {
    customer: {fullName: v.name, phone: v.phone, email: v.email},
    shipping: {line1: v.address, city: v.city},
    note: v.gift ? ('Gift order' + (v.giftMessage ? ' — message: ' + v.giftMessage : '')) : null,
    paymentMethod: 'CashOnDelivery',
    expectedTotal: cartTotal(),
    items: cart.map(i => ({productSlug: i.id, quantity: i.qty})),
  };
  const key = attemptKeyFor(JSON.stringify([body.items, v.phone, v.email, v.address, v.city]));

  setCheckoutBusy(true);
  const ctrl = new AbortController();
  const timer = setTimeout(()=>ctrl.abort(), CHECKOUT_TIMEOUT_MS);
  try{
    const res = await fetch(ORDERS_API_BASE.replace(/\/+$/,'') + '/api/v1/orders', {
      method: 'POST',
      headers: {'Content-Type':'application/json', 'Idempotency-Key': key},
      body: JSON.stringify(body),
      signal: ctrl.signal,
      credentials: 'omit',
    });
    let data = null;
    try{ data = await res.json(); }catch(e){ data = null; }

    if((res.status === 201 || res.status === 200) && isSavedOrder(data)){
      // Only now is the order real: clear the bag and show the server's own numbers.
      cart = []; saveCart(); renderCart();
      checkoutAttempt = null;
      setCheckoutBusy(false);
      showOrderConfirmation(data, v.city);
      return;
    }
    if(res.status === 400 && data && data.errors){
      const fe = {};
      Object.keys(data.errors).forEach(k=>{ const id = CHECKOUT_FIELD_MAP[k]; if(id) fe[id] = [].concat(data.errors[k])[0]; });
      showFieldErrors(fe);
      setCheckoutStatus('Please check the highlighted fields. Your order has not been placed.', 'error');
    } else if(res.status === 409){
      setCheckoutStatus((data && data.code === 'price_changed')
        ? 'Some prices have changed. Please refresh the page and review your bag. Your order has not been placed.'
        : ((data && data.title) || 'Some items are no longer available.') + ' Your order has not been placed.', 'error');
    } else if(res.status === 429){
      setCheckoutStatus('Too many attempts. Please wait a minute and try again. Your order has not been placed.', 'error');
    } else if(res.status === 503){
      setCheckoutStatus('Our order system is not accepting orders right now. Your order has not been placed. Please try again later.', 'error');
    } else {
      setCheckoutStatus('Something went wrong on our side, so your order has not been confirmed. Please try again.', 'error');
    }
  } catch(err){
    setCheckoutStatus(err && err.name === 'AbortError'
      ? 'We could not confirm your order in time. It may not have been placed. Press "Place order" again: it is safe, and you will not get a duplicate order.'
      : 'We could not reach our order system. Your order has NOT been placed. Please check your connection and try again.', 'error');
  } finally {
    clearTimeout(timer);
    if(checkoutBusy) setCheckoutBusy(false);
  }
}

function openCheckout(){
  syncCartFromStorage();
  if(!checkoutModal || cart.length===0){ return; }
  checkoutBusy = false;
  modalBox.innerHTML = checkoutFormHTML();
  checkoutModal.classList.add('show');
  overlay.classList.add('show');

  modalBox.querySelector('#checkoutCloseBtn').addEventListener('click', closeCheckout);
  const giftBox = modalBox.querySelector('#fgift');
  const giftMsg = modalBox.querySelector('#fgiftmsg');
  giftBox.addEventListener('change', ()=>{ giftMsg.hidden = !giftBox.checked; });
  modalBox.querySelector('#checkoutForm').addEventListener('submit', e=>{ e.preventDefault(); submitOrder(); });
}
function closeCheckout(){
  if(!checkoutModal || checkoutBusy) return;                // not while an order is being sent
  checkoutModal.classList.remove('show');
  overlay.classList.remove('show');
}
const checkoutBtnEl = document.getElementById('checkoutBtn');
if(checkoutBtnEl){
  checkoutBtnEl.addEventListener('click', ()=>{
    closeCartFn();
    openCheckout();
  });
}

// Load the saved bag (if any) and show it, then keep it in step with other tabs and the back button.
syncCartFromStorage();
renderCart();
window.addEventListener('storage', e=>{
  if(e.key === null || e.key === CART_STORAGE_KEY){ syncCartFromStorage(); renderCart(); }
});
window.addEventListener('pageshow', e=>{
  if(e.persisted){ syncCartFromStorage(); renderCart(); }
});

// ---------- EMAIL UPDATES SIGNUP ----------
(function(){
  const form = document.getElementById('newsletterForm');
  if(!form) return;
  const input = document.getElementById('newsletterEmail');
  const btn = document.getElementById('newsletterBtn');
  const msg = document.getElementById('newsletterMsg');
  function show(text, kind){
    msg.textContent = text;
    msg.className = 'newsletter-msg show ' + (kind || '');
  }
  form.addEventListener('submit', async (e)=>{
    e.preventDefault();
    const email = input.value.trim();
    if(!/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email)){
      show('Please enter a valid email address.', 'error');
      input.focus();
      return;
    }
    btn.disabled = true; const label = btn.textContent; btn.textContent = 'Subscribing…';
    try{
      const res = await fetch(SUBSCRIBE_ENDPOINT, {
        method: 'POST',
        headers: {'Content-Type':'application/json', 'Accept':'application/json'},
        body: JSON.stringify({
          email: email,
          _honey: document.getElementById('newsletterHoney').value,
          _subject: 'New Kiva Belion subscriber',
          _template: 'table',
          _captcha: 'false',
          _autoresponse: "Thank you for subscribing to Kiva Belion! We'll email you when new fragrances arrive. To unsubscribe at any time, message us on WhatsApp (" + PHONE_DISPLAY + ") or email " + SUBSCRIBE_EMAIL + ". — Kiva Belion, Karachi",
          page: location.href
        })
      });
      const data = await res.json().catch(()=>({}));
      if(res.ok && (data.success === true || data.success === 'true')){
        form.reset();
        show("Thank you — you're subscribed. Check your inbox for a welcome email.", 'ok');
      } else {
        throw new Error('not ok');
      }
    } catch(err){
      show("Sorry, we couldn't sign you up just now. Please try again, or join our updates on WhatsApp.", 'error');
    } finally {
      btn.disabled = false; btn.textContent = label;
    }
  });
})();

// ---------- SCROLL REVEAL ----------
function initScrollReveal(){
  const revealEls = document.querySelectorAll('.reveal');
  if('IntersectionObserver' in window){
    const io = new IntersectionObserver((entries)=>{
      entries.forEach(entry=>{
        if(entry.isIntersecting){
          entry.target.classList.add('in');
          io.unobserve(entry.target);
        }
      });
    }, {threshold:0.15});
    revealEls.forEach(el=>io.observe(el));
  } else {
    revealEls.forEach(el=>el.classList.add('in'));
  }
}
initScrollReveal();

/* ============ MOTION LAYER (small, dependency-free, every part optional) ============ */
(function(){
  try{
    const RM = window.matchMedia('(prefers-reduced-motion: reduce)');
    const header = document.querySelector('header.site-nav');
    const hero = document.querySelector('.hero');
    const video = document.querySelector('.hero-video-wrap video');

    // Visitors who prefer reduced motion do not get an autoplaying background video: the still poster is shown.
    function stopVideo(){
      if(!video) return;
      video.pause(); video.removeAttribute('autoplay');
      video.querySelectorAll('source').forEach(s=>s.remove());
      video.load();
    }
    if(RM.matches) stopVideo();
    if(RM.addEventListener) RM.addEventListener('change', e=>{ if(e.matches) stopVideo(); });

    // Header shadow after scrolling, and a few pixels of hero parallax on desktop (rAF-throttled)
    const fine = window.matchMedia('(min-width: 901px) and (hover: hover)');
    let ticking = false;
    function onScroll(){
      if(ticking) return; ticking = true;
      requestAnimationFrame(()=>{
        ticking = false;
        const y = window.scrollY || 0;
        if(header) header.classList.toggle('scrolled', y > 8);
        if(hero && y < hero.offsetHeight){
          // gentle parallax for everyone who has not asked for reduced motion (smaller travel on phones)
          if(!RM.matches) hero.style.setProperty('--py', Math.min(fine.matches ? 40 : 22, y * (fine.matches ? 0.11 : 0.07)).toFixed(1) + 'px');
          // scroll cue fades away as soon as the visitor scrolls
          hero.style.setProperty('--cue', Math.max(0, 1 - y / 90).toFixed(2));
        }
      });
    }
    window.addEventListener('scroll', onScroll, {passive:true}); onScroll();

    // Fill the first screen: hero height = viewport minus the preview banner + header above it
    function sizeHero(){
      if(!hero) return;
      const top = Math.round(hero.getBoundingClientRect().top + (window.scrollY || 0));
      if(top > 0 && top < 300) hero.style.setProperty('--chrome', top + 'px');
    }
    sizeHero(); window.addEventListener('resize', sizeHero); window.addEventListener('load', sizeHero);
    if(document.fonts && document.fonts.ready) document.fonts.ready.then(sizeHero);

    if('IntersectionObserver' in window){
      // pause the hero's CSS animations while the hero is off-screen
      if(hero) new IntersectionObserver(es=>es.forEach(e=>hero.classList.toggle('is-offscreen', !e.isIntersecting))).observe(hero);
      // section hairlines
      const secIO = new IntersectionObserver(es=>es.forEach(e=>{ if(e.isIntersecting){ e.target.classList.add('in-view'); secIO.unobserve(e.target); } }), {threshold:0.12});
      document.querySelectorAll('.about, .why, .journey, .reviews, .newsletter').forEach(s=>secIO.observe(s));
    }

    // "Added" confirmation on the product-page buttons (card buttons already get it from handleAddClick)
    document.addEventListener('click', e=>{
      const b = e.target.closest && e.target.closest('#detailAddBtn, #soAdd');
      if(!b || b.disabled) return;
      b.classList.add('added');
      setTimeout(()=>b.classList.remove('added'), 1400);
    });

    // Bag badge pops and the subtotal fades when they change
    function watch(id, cls){
      const el = document.getElementById(id); if(!el) return;
      new MutationObserver(()=>{
        el.classList.remove(cls); void el.offsetWidth; el.classList.add(cls);
      }).observe(el, {childList:true, characterData:true, subtree:true});
      el.addEventListener('animationend', ()=>el.classList.remove(cls));
    }
    watch('cartCount', 'bump'); watch('cartSubtotal', 'tick');

    // Bag drawer: staggered items on open, focus moves in on open and back to the bag button on close
    const drawer = document.getElementById('cartDrawer');
    if(drawer){
      let was = drawer.classList.contains('show');
      new MutationObserver(()=>{
        const now = drawer.classList.contains('show'); if(now === was) return; was = now;
        if(now){
          drawer.classList.add('is-opening'); setTimeout(()=>drawer.classList.remove('is-opening'), 900);
          const c = document.getElementById('closeCart'); if(c) c.focus({preventScroll:true});
        } else if(!document.querySelector('.modal.show')){
          const b = document.getElementById('cartBtn'); if(b) b.focus({preventScroll:true});
        }
      }).observe(drawer, {attributes:true, attributeFilter:['class']});
    }
    // Checkout dialog: focus the first field when it opens
    const modal = document.getElementById('checkoutModal');
    if(modal){
      let was = modal.classList.contains('show');
      new MutationObserver(()=>{
        const now = modal.classList.contains('show'); if(now === was) return; was = now;
        if(now){ const f = modal.querySelector('#fname') || modal.querySelector('button'); if(f) f.focus({preventScroll:true}); }
        else { const b = document.getElementById('cartBtn'); if(b) b.focus({preventScroll:true}); }
      }).observe(modal, {attributes:true, attributeFilter:['class']});
    }

    // Menu button announces open/closed; Escape closes the checkout, the bag or the menu
    const nl = document.getElementById('navLinks'), mt = document.getElementById('menuToggle');
    if(nl && mt){
      mt.setAttribute('aria-expanded', 'false');
      new MutationObserver(()=>mt.setAttribute('aria-expanded', nl.classList.contains('show') ? 'true' : 'false'))
        .observe(nl, {attributes:true, attributeFilter:['class']});
    }
    // Keep Tab inside the bag / checkout dialog while it is open
    document.addEventListener('keydown', e=>{
      if(e.key !== 'Tab') return;
      const box = (modal && modal.classList.contains('show')) ? modal : (drawer && drawer.classList.contains('show') ? drawer : null);
      if(!box) return;
      const f = Array.prototype.filter.call(box.querySelectorAll('a[href], button, input, textarea, select, [tabindex]:not([tabindex="-1"])'),
        el => !el.disabled && !el.hidden && el.offsetParent !== null && el.getAttribute('aria-disabled') !== 'true');
      if(!f.length) return;
      const first = f[0], last = f[f.length-1];
      if(!box.contains(document.activeElement)){ e.preventDefault(); first.focus(); }
      else if(e.shiftKey && document.activeElement === first){ e.preventDefault(); last.focus(); }
      else if(!e.shiftKey && document.activeElement === last){ e.preventDefault(); first.focus(); }
    });
    // Tap or click outside the open mobile menu closes it
    document.addEventListener('click', e=>{
      if(!nl || !nl.classList.contains('show')) return;
      if(nl.contains(e.target) || (mt && mt.contains(e.target))) return;
      nl.classList.remove('show');
    });
    // Skip link: move focus to the main content
    document.addEventListener('click', e=>{
      const s = e.target.closest && e.target.closest('.skip-link'); if(!s) return;
      const m = document.getElementById('main'); if(m){ e.preventDefault(); m.focus({preventScroll:false}); m.scrollIntoView(); }
    });
    document.addEventListener('keydown', e=>{
      if(e.key !== 'Escape') return;
      if(modal && modal.classList.contains('show')) closeCheckout();
      else if(drawer && drawer.classList.contains('show')) closeCartFn();
      else if(nl && nl.classList.contains('show')){ nl.classList.remove('show'); if(mt) mt.focus(); }
    });
  }catch(err){ /* motion is optional; never let it break the shop */ }
})();
