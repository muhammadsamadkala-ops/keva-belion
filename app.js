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
            <button class="qty-btn" onclick="changeQty('${i.id}',-1)">−</button>
            <span>${i.qty}</span>
            <button class="qty-btn" onclick="changeQty('${i.id}',1)">+</button>
            <a class="remove-link" onclick="removeItem('${i.id}')">Remove</a>
          </div>
        </div>
      </div>
    `).join('');
  }
  const subtotalEl = document.getElementById('cartSubtotal');
  if(subtotalEl) subtotalEl.textContent = 'PKR ' + cartTotal().toLocaleString();
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

// ---------- CHECKOUT ----------
const checkoutModal = document.getElementById('checkoutModal');
const modalBox = document.getElementById('modalBox');

function checkoutFormHTML(){
  return `
    <button type="button" class="modal-close-btn" id="checkoutCloseBtn" aria-label="Close checkout">&times;</button>
    <h3>Checkout</h3>
    <div class="sub">${cart.length} item${cart.length!==1?'s':''} · PKR ${cartTotal().toLocaleString()}</div>
    <form id="checkoutForm">
      <div class="field">
        <label for="fname">Full name</label>
        <input type="text" id="fname" required>
      </div>
      <div class="field">
        <label for="fphone">Phone number</label>
        <input type="tel" id="fphone" required placeholder="03XX XXXXXXX">
      </div>
      <div class="field">
        <label for="faddress">Delivery address</label>
        <input type="text" id="faddress" required>
      </div>
      <div class="field">
        <label for="fcity">City</label>
        <input type="text" id="fcity" required placeholder="Karachi">
      </div>
      <div class="field gift-field">
        <label class="gift-check"><input type="checkbox" id="fgift"> This order is a gift</label>
        <textarea id="fgiftmsg" rows="2" maxlength="200" placeholder="Message for the recipient (optional)" hidden></textarea>
      </div>
      <div class="field">
        <label>Payment method</label>
        <div class="pay-options">
          <div class="pay-option active" data-method="cod">Cash on Delivery</div>
          <div class="pay-option disabled" aria-disabled="true" title="Card payments are not available yet">
            <span class="pay-option-icons">
              <svg viewBox="0 0 24 16" width="26" height="17"><rect x="0.5" y="0.5" width="23" height="15" rx="2.5" fill="#fff" stroke="var(--line)"/><rect x="0.5" y="4" width="23" height="3" fill="var(--gold)"/><rect x="2.5" y="10.5" width="6" height="2" rx="1" fill="var(--text-muted)"/></svg>
            </span>
            Card · coming soon
          </div>
        </div>
        <p class="pay-note">Want to pay by bank transfer instead? Order on WhatsApp and we'll share the account details.</p>
      </div>
      <button type="submit" class="btn btn-primary modal-submit">Send order on WhatsApp</button>
    </form>
  `;
}

function openCheckout(){
  syncCartFromStorage();
  if(!checkoutModal || cart.length===0){ return; }
  modalBox.innerHTML = checkoutFormHTML();
  checkoutModal.classList.add('show');
  overlay.classList.add('show');

  modalBox.querySelector('#checkoutCloseBtn').addEventListener('click', closeCheckout);

  const giftBox = modalBox.querySelector('#fgift');
  const giftMsg = modalBox.querySelector('#fgiftmsg');
  giftBox.addEventListener('change', ()=>{ giftMsg.hidden = !giftBox.checked; });

  modalBox.querySelector('#checkoutForm').addEventListener('submit', e=>{
    e.preventDefault();
    // Use the latest saved bag (it may have changed in another tab). If it is now empty there is nothing to send.
    syncCartFromStorage();
    if(cart.length === 0){ closeCheckout(); renderCart(); return; }
    const customer = {
      name: modalBox.querySelector('#fname').value.trim(),
      phone: modalBox.querySelector('#fphone').value.trim(),
      address: modalBox.querySelector('#faddress').value.trim(),
      city: modalBox.querySelector('#fcity').value.trim(),
      gift: modalBox.querySelector('#fgift').checked,
      giftMessage: modalBox.querySelector('#fgiftmsg').value.trim(),
    };
    const items = cart.map(i => ({name:i.name, price:i.price, qty:i.qty}));
    const total = cartTotal();
    const waUrl = buildWhatsAppOrderUrl(items, customer);
    // Open WhatsApp straight away (this runs inside the user's click, so it is not blocked)
    window.open(waUrl, '_blank', 'noopener');
    modalBox.innerHTML = `
      <button type="button" class="modal-close-btn" id="checkoutCloseBtn2" aria-label="Close">&times;</button>
      <div class="order-confirm">
        <div class="check-circle">→</div>
        <h3>One last step</h3>
        <p>Your order is ready — <strong>it is only confirmed once you send the message in WhatsApp.</strong></p>
        <p>Total: <strong style="color:var(--gold-bright)">PKR ${total.toLocaleString()}</strong> · Cash on Delivery</p>
        <p style="margin-top:14px;">WhatsApp didn't open? Tap the button below.</p>
        <a class="btn btn-whatsapp" style="margin-top:14px;" href="${waUrl}" target="_blank" rel="noopener">Open WhatsApp</a>
        <p class="confirm-warning" role="note" style="margin-top:16px; padding:10px 12px; border-left:3px solid var(--coral); background:rgba(217,68,48,.07); color:var(--text); text-align:left; font-size:13.5px;"><strong>Before you tap a button:</strong> tap "I've sent my order" only after you have sent the message in WhatsApp. If you leave without sending it, or without tapping the button, your bag stays saved on this device. Check your bag next time so the same order is not sent twice.</p>
        <div><button class="btn btn-primary" style="margin-top:12px;" id="sentConfirm">I've sent my order</button></div>
        <div><button class="btn btn-ghost" style="margin-top:12px;" id="closeConfirm">Continue shopping</button></div>
      </div>
    `;
    // The bag is kept until the customer says the order was sent. Closing this screen does not clear it.
    modalBox.querySelector('#sentConfirm').addEventListener('click', ()=>{
      cart = [];
      saveCart();
      renderCart();
      closeCheckout();
    });
    modalBox.querySelector('#closeConfirm').addEventListener('click', closeCheckout);
    modalBox.querySelector('#checkoutCloseBtn2').addEventListener('click', closeCheckout);
  });
}
function closeCheckout(){
  if(!checkoutModal) return;
  checkoutModal.classList.remove('show');
  overlay.classList.remove('show');
}
const waOrderBtnEl = document.getElementById('waOrderBtn');
if(waOrderBtnEl){
  waOrderBtnEl.addEventListener('click', ()=>{
    if(cart.length === 0){
      const empty = document.querySelector('.cart-empty');
      if(empty) empty.style.color = 'var(--coral)';
      return;
    }
    window.open(buildWhatsAppOrderUrl(cart), '_blank', 'noopener');
  });
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
