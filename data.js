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
  {id:'noir', name:'Noir', notes:'Oud · Amber · Spice', price:8900, image:'assets/products/noir.jpg',
   desc:'Our darkest, most commanding scent. Smoky oud and warm amber wrapped in a spiced base — bold enough for winter nights, refined enough for daily wear.'},
  {id:'blanc', name:'Blanc', notes:'White Florals · Musk', price:7200, image:'assets/products/blanc.jpg',
   desc:'A luminous white-floral built around jasmine and clean musk. Soft, confident, and easy to wear from morning to evening.'},
  {id:'ocean', name:'Ocean', notes:'Marine · Citrus · Ambergris', price:7800, image:'assets/products/ocean.jpg',
   desc:'Fresh marine notes over bright citrus, settling into a soft ambergris base. Our most refreshing scent — built for warm days.'},
  {id:'rouge', name:'Rouge', notes:'Rose · Berries · Spice', price:8200, image:'assets/products/rouge.jpg',
   desc:'Deep rose and dark berries warmed with a touch of spice. Romantic and rich — the kind of scent that lingers on a scarf.'},
  {id:'vert', name:'Vert', notes:'Green Notes · Fig · Vetiver', price:7500, image:'assets/products/vert.jpg',
   desc:'Crisp green notes and fig, grounded in earthy vetiver. Clean, natural, and quietly confident — our take on a modern green scent.'},
  {id:'amber', name:'Amber', notes:'Amber · Vanilla · Woods', price:8500, image:'assets/products/amber.jpg',
   desc:'Warm amber and soft vanilla over a woody base. Cosy and enveloping — built to last long after the sun goes down.'},
  {id:'fleur', name:'Fleur', notes:'Peony · Pink Florals · Musk', price:7400, image:'assets/products/fleur.jpg',
   desc:'Soft peony and pink florals over a gentle musk base. Light, romantic, and effortless — an everyday floral that never feels heavy.'},
  {id:'elegance', name:'Élégance', notes:'Iris · White Musk · Woods', price:9200, image:'assets/products/elegance.jpg',
   desc:'Powdery iris and clean white musk over refined woods. Understated and sophisticated — our most versatile, unisex composition.'},
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

function bottleVisualHTML(p){
  if(p.image){
    return `<div class="bottle-wrap photo-wrap"><img src="${p.image}" alt="${p.name}" loading="lazy"></div>`;
  }
  return `<div class="bottle-wrap liquid-${p.id}"><svg viewBox="0 0 120 240"><use href="#bottle-shape"/></svg></div>`;
}

function renderProductCard(p, container, linkToDetail){
  const card = document.createElement('div');
  card.className = 'product-card reveal';
  const visual = bottleVisualHTML(p);
  const inner = `
    ${visual}
    ${p.inspiredBy ? `<div class="inspired-tag">Inspired by ${p.inspiredBy}</div>` : ''}
    <h3>${p.name}</h3>
    <div class="notes">${p.notes}</div>
    <div class="price">PKR ${p.price.toLocaleString()}</div>
    <button class="add-btn" data-id="${p.id}">Add to bag</button>
  `;
  if(linkToDetail){
    card.innerHTML = `<a href="product.html?id=${p.id}" style="display:block; color:inherit;">
      ${visual}
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
