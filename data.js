/* ============ SHARED PRODUCT DATA ============ */
const LIQUID = {
  noor:      'rgba(232,224,200,0.45)',
  zaroud:    'rgba(179,107,42,0.55)',
  gulnar:    'rgba(224,110,110,0.45)',
  shab:      'rgba(90,55,80,0.55)',
  ambergris: 'rgba(150,100,60,0.5)',
  citrine:   'rgba(200,196,90,0.45)',
  sauvagenoir: 'rgba(150,180,190,0.4)',
  bleuele:     'rgba(70,100,140,0.45)',
  oudreserve:  'rgba(120,70,40,0.5)',
  blackopu:    'rgba(70,50,45,0.55)',
  baccbloom:   'rgba(200,90,60,0.45)',
  aventusn:    'rgba(90,110,80,0.45)'
};

const PRODUCTS = [
  {id:'noor', name:'Noor', notes:'White florals · Musk', price:7200,
   desc:'A luminous white-floral built around jasmine and clean musk, warmed with a whisper of amber. Soft, confident, and easy to wear from morning to evening.'},
  {id:'zaroud', name:'Zar-e-Oud', notes:'Oud · Amber', price:8900,
   desc:'Our signature oud — deep, resinous and smoky, rounded out with warm amber and a touch of spice. Bold enough for winter evenings, refined enough for daily wear.'},
  {id:'gulnar', name:'Gulnar', notes:'Rose · Saffron', price:8200,
   desc:'A modern take on rose, brightened with saffron and a soft powdery base. Romantic without being heavy — the kind of rose that lingers on a scarf.'},
  {id:'shab', name:'Shab', notes:'Dark vanilla · Tonka', price:7800,
   desc:'Dark vanilla and tonka bean, wrapped in a soft gourmand base. Cosy, sensual, and built to last long after the sun goes down — the name means "night".'},
  {id:'ambergris', name:'Ambergris Nuit', notes:'Amber · Leather', price:9500,
   desc:'Amber and supple leather sit at the heart of this one, with a smoky base that develops for hours. Our richest, most enduring composition.'},
  {id:'citrine', name:'Citrine Bloom', notes:'Citrus · Jasmine', price:6900,
   desc:'Bright citrus opens into a heart of jasmine and light musk. An everyday scent that feels sunny without ever tipping into sweet.'},
];

const INSPIRED = [
  {id:'sauvagenoir', name:'Sauvage Noir', inspiredBy:'Dior Sauvage', notes:'Bergamot · Ambroxan', price:5500,
   desc:'Our interpretation of the modern fresh-spicy classic — sharp bergamot up top, settling into a clean, radiant ambroxan base. Long-lasting and versatile.'},
  {id:'bleuele', name:'Bleu Elegance', inspiredBy:'Chanel Bleu de Chanel', notes:'Citrus · Woods', price:5500,
   desc:'Citrus and aromatic woods in a clean, confident composition inspired by one of the most iconic men\'s fragrances. Sharp, elegant, and office-appropriate.'},
  {id:'oudreserve', name:'Oud Wood Reserve', inspiredBy:'Tom Ford Oud Wood', notes:'Oud · Sandalwood', price:6500,
   desc:'Smooth oud layered over creamy sandalwood and a hint of spice — our take on a luxury house classic, bottled at a fraction of the price.'},
  {id:'blackopu', name:'Black Opulence', inspiredBy:'YSL Black Opium', notes:'Coffee · Vanilla', price:5900,
   desc:'Dark coffee and sweet vanilla collide in this bold, addictive evening scent inspired by a modern icon. Not for the faint-hearted.'},
  {id:'baccbloom', name:'Baccarat Bloom', inspiredBy:'MFK Baccarat Rouge 540', notes:'Saffron · Amber', price:6900,
   desc:'Saffron, amber and a crystalline musk base — our tribute to one of the most talked-about niche fragrances of the decade.'},
  {id:'aventusn', name:'Aventus Noir', inspiredBy:'Creed Aventus', notes:'Pineapple · Birch', price:6900,
   desc:'Fruity top notes over smoky birch and musk — inspired by the fragrance that changed how the world thinks about "confidence in a bottle".'},
];

const ALL_PRODUCTS = PRODUCTS.concat(INSPIRED);

function getProductById(id){
  return ALL_PRODUCTS.find(p=>p.id===id);
}

// inject dynamic liquid-fill CSS rules once per page
(function injectLiquidStyles(){
  const styleTag = document.createElement('style');
  document.head.appendChild(styleTag);
  let rules = '';
  Object.keys(LIQUID).forEach(k=>{
    rules += `.liquid-${k} .bottle-body{ fill:${LIQUID[k]}; }\n`;
  });
  styleTag.textContent = rules;
})();

function renderProductCard(p, container, linkToDetail){
  const card = document.createElement('div');
  card.className = 'product-card reveal';
  const inner = `
    <div class="bottle-wrap liquid-${p.id}">
      <svg viewBox="0 0 120 240"><use href="#bottle-shape"/></svg>
    </div>
    ${p.inspiredBy ? `<div class="inspired-tag">Inspired by ${p.inspiredBy}</div>` : ''}
    <h3>${p.name}</h3>
    <div class="notes">${p.notes}</div>
    <div class="price">PKR ${p.price.toLocaleString()}</div>
    <button class="add-btn" data-id="${p.id}">Add to bag</button>
  `;
  if(linkToDetail){
    card.innerHTML = `<a href="product.html?id=${p.id}" style="display:block; color:inherit;">
      <div class="bottle-wrap liquid-${p.id}">
        <svg viewBox="0 0 120 240"><use href="#bottle-shape"/></svg>
      </div>
      ${p.inspiredBy ? `<div class="inspired-tag">Inspired by ${p.inspiredBy}</div>` : ''}
      <h3>${p.name}</h3>
      <div class="notes">${p.notes}</div>
      <div class="price">PKR ${p.price.toLocaleString()}</div>
    </a>
    <button class="add-btn" data-id="${p.id}">Add to bag</button>`;
  } else {
    card.innerHTML = inner;
  }
  container.appendChild(card);
}
