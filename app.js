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
          <div class="pay-option" data-method="card">Debit / Credit Card</div>
        </div>
        <div class="bank-details" id="cardDetails">
          <div class="field" style="margin-bottom:12px;">
            <label for="cardNumber">Card number</label>
            <input type="text" id="cardNumber" inputmode="numeric" maxlength="19" placeholder="1234 5678 9012 3456">
          </div>
          <div style="display:flex; gap:12px;">
            <div class="field" style="flex:1; margin-bottom:12px;">
              <label for="cardExpiry">Expiry</label>
              <input type="text" id="cardExpiry" maxlength="5" placeholder="MM/YY">
            </div>
            <div class="field" style="flex:1; margin-bottom:12px;">
              <label for="cardCvv">CVV</label>
              <input type="text" id="cardCvv" inputmode="numeric" maxlength="4" placeholder="123">
            </div>
          </div>
          <div class="field" style="margin-bottom:0;">
            <label for="cardName">Name on card</label>
            <input type="text" id="cardName" placeholder="As shown on card">
          </div>
        </div>
      </div>
      <button type="submit" class="btn btn-primary modal-submit">Place order</button>
    </form>
  `;
}

function openCheckout(){
  if(!checkoutModal || cart.length===0){ return; }
  modalBox.innerHTML = checkoutFormHTML();
  checkoutModal.classList.add('show');
  overlay.classList.add('show');

  modalBox.querySelector('#checkoutCloseBtn').addEventListener('click', closeCheckout);

  const options = modalBox.querySelectorAll('.pay-option');
  const cardDetails = modalBox.querySelector('#cardDetails');
  let method = 'cod';
  options.forEach(opt=>{
    opt.addEventListener('click', ()=>{
      options.forEach(o=>o.classList.remove('active'));
      opt.classList.add('active');
      method = opt.dataset.method;
      cardDetails.classList.toggle('show', method==='card');
      cardDetails.querySelectorAll('input').forEach(inp=>{ inp.required = (method==='card'); });
    });
  });

  // light auto-formatting for card fields
  const cardNumberEl = modalBox.querySelector('#cardNumber');
  if(cardNumberEl){
    cardNumberEl.addEventListener('input', ()=>{
      cardNumberEl.value = cardNumberEl.value.replace(/\D/g,'').slice(0,16).replace(/(.{4})/g,'$1 ').trim();
    });
  }
  const cardExpiryEl = modalBox.querySelector('#cardExpiry');
  if(cardExpiryEl){
    cardExpiryEl.addEventListener('input', ()=>{
      let v = cardExpiryEl.value.replace(/\D/g,'').slice(0,4);
      if(v.length>=3) v = v.slice(0,2) + '/' + v.slice(2);
      cardExpiryEl.value = v;
    });
  }
  const cardCvvEl = modalBox.querySelector('#cardCvv');
  if(cardCvvEl){
    cardCvvEl.addEventListener('input', ()=>{
      cardCvvEl.value = cardCvvEl.value.replace(/\D/g,'').slice(0,4);
    });
  }

  modalBox.querySelector('#checkoutForm').addEventListener('submit', e=>{
    e.preventDefault();
    const orderId = 'KB-' + Math.floor(100000 + Math.random()*900000);
    const total = cartTotal();
    modalBox.innerHTML = `
      <button type="button" class="modal-close-btn" id="checkoutCloseBtn2" aria-label="Close">&times;</button>
      <div class="order-confirm">
        <div class="check-circle">✓</div>
        <h3>Order placed</h3>
        <p>Thank you — your order <span class="oid">${orderId}</span> has been received.</p>
        <p>Total: <strong style="color:var(--gold-bright)">PKR ${total.toLocaleString()}</strong></p>
        <p style="margin-top:14px;">${method==='cod' ? "We'll call you shortly to confirm your Cash on Delivery order." : "Your card payment is being verified — you'll receive a confirmation shortly."}</p>
        <button class="btn btn-ghost" style="margin-top:22px;" id="closeConfirm">Continue shopping</button>
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

// ---------- COUNT UP ----------
function animateCount(el){
  const target = parseFloat(el.dataset.count);
  const prefix = el.dataset.prefix || '';
  const suffix = el.dataset.suffix || '';
  const duration = 1200;
  const start = performance.now();
  function step(now){
    const progress = Math.min((now-start)/duration, 1);
    const eased = 1 - Math.pow(1-progress, 3);
    const val = Math.round(target*eased);
    el.textContent = prefix + val + suffix;
    if(progress<1){ requestAnimationFrame(step); }
  }
  requestAnimationFrame(step);
}
const statEls = document.querySelectorAll('.about-stats strong[data-count]');
if('IntersectionObserver' in window && statEls.length){
  const statIo = new IntersectionObserver((entries)=>{
    entries.forEach(entry=>{
      if(entry.isIntersecting){
        if(reduceMotion){
          const el = entry.target;
          el.textContent = (el.dataset.prefix||'') + el.dataset.count + (el.dataset.suffix||'');
        } else {
          animateCount(entry.target);
        }
        statIo.unobserve(entry.target);
      }
    });
  }, {threshold:0.4});
  statEls.forEach(el=>statIo.observe(el));
}
