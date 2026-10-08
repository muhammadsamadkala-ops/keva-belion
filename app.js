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

// ---------- CART STATE (in-memory only, shared across page via localStorage-free session) ----------
let cart = [];

function addToCart(id, qty){
  qty = qty || 1;
  const existing = cart.find(i=>i.id===id);
  if(existing){ existing.qty += qty; } else {
    const p = getProductById(id);
    if(!p) return;
    cart.push({...p, qty:qty});
  }
  renderCart();
}
function changeQty(id, delta){
  const item = cart.find(i=>i.id===id);
  if(!item) return;
  item.qty += delta;
  if(item.qty<=0){ cart = cart.filter(i=>i.id!==id); }
  renderCart();
}
function removeItem(id){
  cart = cart.filter(i=>i.id!==id);
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
  if(!checkoutModal || cart.length===0){ return; }
  modalBox.innerHTML = checkoutFormHTML();
  checkoutModal.classList.add('show');
  overlay.classList.add('show');

  modalBox.querySelector('#checkoutCloseBtn').addEventListener('click', closeCheckout);

  modalBox.querySelector('#checkoutForm').addEventListener('submit', e=>{
    e.preventDefault();
    const customer = {
      name: modalBox.querySelector('#fname').value.trim(),
      phone: modalBox.querySelector('#fphone').value.trim(),
      address: modalBox.querySelector('#faddress').value.trim(),
      city: modalBox.querySelector('#fcity').value.trim(),
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
        <div><button class="btn btn-ghost" style="margin-top:12px;" id="closeConfirm">Continue shopping</button></div>
      </div>
    `;
    cart = [];
    renderCart();
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

renderCart();

// ---------- NEWSLETTER ----------
const newsletterForm = document.getElementById('newsletterForm');
if(newsletterForm){
  newsletterForm.addEventListener('submit', e=>{
    e.preventDefault();
    document.getElementById('newsletterMsg').classList.add('show');
    newsletterForm.reset();
  });
}

// ---------- CURSOR GLOW ----------
const cursorGlow = document.getElementById('cursorGlow');
const heroSection = document.querySelector('.hero');
if(cursorGlow && heroSection && !window.matchMedia('(max-width:900px)').matches){
  heroSection.addEventListener('mousemove', (e)=>{
    const rect = heroSection.getBoundingClientRect();
    cursorGlow.style.left = (e.clientX - rect.left) + 'px';
    cursorGlow.style.top = (e.clientY - rect.top) + 'px';
  });
}

// ---------- SPARKLES ----------
const sparkleWrap = document.getElementById('sparkles');
const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
if(sparkleWrap && !reduceMotion){
  for(let i=0;i<22;i++){
    const s = document.createElement('i');
    s.style.left = Math.random()*100 + '%';
    s.style.top = Math.random()*70 + '%';
    s.style.animationDelay = (Math.random()*3.4) + 's';
    s.style.animationDuration = (2.6 + Math.random()*2.4) + 's';
    sparkleWrap.appendChild(s);
  }
}

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
