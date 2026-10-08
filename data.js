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
   gender:'men', family:'woody', intensity:'bold',
   desc:'Our darkest, most commanding scent. Smoky oud and warm amber wrapped in a spiced base — bold enough for winter nights, refined enough for daily wear.'},
  {id:'blanc', name:'Blanc', notes:'White Florals · Musk', price:7200, compareAtPrice:8000, image:'assets/products/blanc.jpg',
   gender:'women', family:'floral', intensity:'light',
   desc:'A luminous white-floral built around jasmine and clean musk. Soft, confident, and easy to wear from morning to evening.'},
  {id:'ocean', name:'Ocean', notes:'Marine · Citrus · Ambergris', price:7800, image:'assets/products/ocean.jpg',
   gender:'men', family:'fresh', intensity:'light',
   desc:'Fresh marine notes over bright citrus, settling into a soft ambergris base. Our most refreshing scent — built for warm days.'},
  {id:'rouge', name:'Rouge', notes:'Rose · Berries · Spice', price:8200, image:'assets/products/rouge.jpg',
   gender:'women', family:'floral', intensity:'bold',
   desc:'Deep rose and dark berries warmed with a touch of spice. Romantic and rich — one for evenings and special occasions.'},
  {id:'vert', name:'Vert', notes:'Green Notes · Fig · Vetiver', price:7500, compareAtPrice:8300, image:'assets/products/vert.jpg',
   gender:'unisex', family:'fresh', intensity:'light',
   desc:'Crisp green notes and fig, grounded in earthy vetiver. Clean, natural, and quietly confident — our take on a modern green scent.'},
  {id:'amber', name:'Amber', notes:'Amber · Vanilla · Woods', price:8500, image:'assets/products/amber.jpg',
   gender:'unisex', family:'sweet', intensity:'bold',
   desc:'Warm amber and soft vanilla over a woody base. Cosy and enveloping — a natural fit for evenings.'},
  {id:'fleur', name:'Fleur', notes:'Peony · Pink Florals · Musk', price:7400, compareAtPrice:8200, image:'assets/products/fleur.jpg',
   gender:'women', family:'floral', intensity:'light',
   desc:'Soft peony and pink florals over a gentle musk base. Light, romantic, and effortless — an everyday floral that never feels heavy.'},
  {id:'elegance', name:'Élégance', notes:'Iris · White Musk · Woods', price:9200, image:'assets/products/elegance.jpg',
   gender:'unisex', family:'woody', intensity:'light',
   desc:'Powdery iris and clean white musk over refined woods. Understated and sophisticated — our most versatile, unisex composition.'},
];

const INSPIRED = [
  {id:'sauvagenoir', name:'Sauvage Noir', inspiredBy:'Dior Sauvage', notes:'Bergamot · Ambroxan', price:5500,
   gender:'men', family:'fresh', intensity:'bold',
   desc:'Our interpretation of the modern fresh-spicy classic — sharp bergamot up top, settling into a clean, radiant ambroxan base. Versatile and easy to wear.'},
  {id:'bleuele', short:'Citrus and aromatic woods — clean, sharp and office-friendly.', name:'Bleu Elegance', inspiredBy:'Chanel Bleu de Chanel', notes:'Citrus · Woods', price:5500, compareAtPrice:6200,
   gender:'men', family:'fresh', intensity:'bold',
   desc:'Citrus and aromatic woods in a clean, confident composition inspired by one of the most iconic men\'s fragrances. Sharp, elegant, and office-appropriate.'},
  {id:'oudreserve', name:'Oud Wood Reserve', inspiredBy:'Tom Ford Oud Wood', notes:'Oud · Sandalwood', price:6500,
   gender:'men', family:'woody', intensity:'bold',
   desc:'Smooth oud layered over creamy sandalwood and a hint of spice — our take on a luxury house classic, bottled at a fraction of the price.'},
  {id:'blackopu', short:'Dark coffee and sweet vanilla — a bold evening scent.', name:'Black Opulence', inspiredBy:'YSL Black Opium', notes:'Coffee · Vanilla', price:5900,
   gender:'women', family:'sweet', intensity:'bold',
   desc:'Dark coffee and sweet vanilla collide in this bold, addictive evening scent inspired by a modern icon. Not for the faint-hearted.'},
  {id:'baccbloom', name:'Baccarat Bloom', inspiredBy:'MFK Baccarat Rouge 540', notes:'Saffron · Amber', price:6900,
   gender:'unisex', family:'sweet', intensity:'bold',
   desc:'Saffron, amber and a crystalline musk base — our tribute to one of the most talked-about niche fragrances of the decade.'},
  {id:'aventusn', name:'Aventus Noir', inspiredBy:'Creed Aventus', notes:'Pineapple · Birch', price:6900, compareAtPrice:7600,
   gender:'men', family:'fresh', intensity:'bold',
   desc:'Fruity top notes over smoky birch and musk — inspired by the fragrance that changed how the world thinks about "confidence in a bottle".'},
];

const ALL_PRODUCTS = PRODUCTS.concat(INSPIRED);

/* ============ FRAGRANCE PYRAMID (Top / Heart / Base) ============
   NOTE FOR OWNER: these are working notes derived from each scent's description.
   Replace them with your real formula notes before launch. */
const PYRAMIDS = {
  noir:        {top:['Spice'],          heart:['Oud'],                     base:['Amber']},
  blanc:       {top:['Citrus Zest'],    heart:['Jasmine','White Florals'],  base:['Clean Musk']},
  ocean:       {top:['Citrus'],         heart:['Marine Accord'],            base:['Ambergris']},
  rouge:       {top:['Dark Berries'],   heart:['Rose'],                     base:['Warm Spice']},
  vert:        {top:['Green Notes'],    heart:['Fig'],                      base:['Vetiver']},
  amber:       {top:['Bergamot'],       heart:['Vanilla'],                  base:['Amber','Woods']},
  fleur:       {top:['Peony'],          heart:['Pink Florals'],             base:['Soft Musk']},
  elegance:    {top:['Soft Citrus'],    heart:['Iris'],                     base:['White Musk','Woods']},
  sauvagenoir: {top:['Bergamot'],       heart:['Spice'],                    base:['Ambroxan']},
  bleuele:     {top:['Citrus'],         heart:['Aromatic Herbs'],           base:['Woods']},
  oudreserve:  {top:['Spice'],          heart:['Oud'],                      base:['Sandalwood']},
  blackopu:    {top:['Coffee'],         heart:['Floral Accord'],            base:['Vanilla']},
  baccbloom:   {top:['Saffron'],        heart:['Jasmine'],                  base:['Amber','Musk']},
  aventusn:    {top:['Pineapple'],      heart:['Birch'],                    base:['Musk']},
};
ALL_PRODUCTS.forEach(p => { p.pyramid = PYRAMIDS[p.id] || null; });

const FAMILY_LABEL = {fresh:'Fresh', floral:'Floral', sweet:'Warm & Sweet', woody:'Woody'};
const GENDER_LABEL = {men:'For him', women:'For her', unisex:'Unisex'};
const INTENSITY_LABEL = {light:'Light — everyday', bold:'Bold — statement'};

function getProductById(id){
  return ALL_PRODUCTS.find(p=>p.id===id);
}

// inject dynamic liquid-fill CSS rules once per page
(function injectLiquidStyles(){
  const styleTag = document.createElement('style');
  document.head.appendChild(styleTag);
  let rules = '';
  Object.keys(LIQUID).forEach(k=>{
    rules += `.liquid-${k}{ --liquid:${LIQUID[k]}; }\n`;
  });
  styleTag.textContent = rules;
})();

function bottleVisualHTML(p){
  const badge = p.compareAtPrice ? `<div class="sale-badge">Sale</div>` : '';
  if(p.image){
    return `<div class="bottle-wrap photo-wrap">${badge}<img src="${p.image}" alt="${p.name}" loading="lazy"></div>`;
  }
  return `<div class="bottle-wrap liquid-${p.id}">${badge}<svg viewBox="0 0 120 240"><use href="#bottle-shape"/></svg></div>`;
}

function priceHTML(p){
  if(p.compareAtPrice && p.compareAtPrice > p.price){
    return `<div class="price">PKR ${p.price.toLocaleString()} <span class="compare-price">PKR ${p.compareAtPrice.toLocaleString()}</span></div>`;
  }
  return `<div class="price">PKR ${p.price.toLocaleString()}</div>`;
}

function shortDesc(p){
  if(p.short) return p.short;
  if(!p.desc) return '';
  const max = 100;
  const sentences = p.desc.match(/[^.!?]+[.!?]+(\s|$)/g) || [p.desc];
  let out = '';
  for(const s of sentences){
    if((out + s).trim().length <= max) out += s; else break;
  }
  out = out.trim();
  if(out) return out;
  const clause = p.desc.split(' — ')[0].trim();
  if(clause.length >= 25 && clause.length <= max) return clause.replace(/[,;:]$/, '') + '.';
  return p.desc.slice(0, max).replace(/\s+\S*$/, '') + '…';
}

function renderProductCard(p, container){
  const card = document.createElement('div');
  card.className = 'product-card reveal';
  const visual = bottleVisualHTML(p);
  const priceBlock = priceHTML(p);
  card.innerHTML = `
    <a href="product.html?id=${p.id}" class="card-media-link">
      ${visual}
      ${p.inspiredBy ? `<div class="inspired-tag">Inspired by ${p.inspiredBy}</div>` : ''}
      <h3>${p.name}</h3>
      <div class="notes">${p.notes}</div>
      <div class="size-tag">100 ML</div>
      <p class="card-desc">${shortDesc(p)}</p>
      ${priceBlock}
    </a>
    <div class="card-actions">
      <a href="product.html?id=${p.id}" class="view-details-btn">View Details</a>
      <button class="add-btn" data-id="${p.id}">Add to Cart</button>
    </div>
  `;
  container.appendChild(card);
}

/* ============ WHATSAPP ORDER MESSAGE ============ */
// items: array of {name, price, qty}. Uses WA_BASE from partials.js
function buildWhatsAppOrderUrl(items, customer){
  const lines = items.map((it, i) =>
    `${i+1}. ${it.name} (100 ML) x${it.qty} — PKR ${(it.price * it.qty).toLocaleString()}`
  );
  const total = items.reduce((s, it) => s + it.price * it.qty, 0);
  const c = customer || {};
  const msg =
`Assalam o Alaikum! I'd like to order from Kiva Belion:

${lines.join('\n')}

Total: PKR ${total.toLocaleString()}

Name: ${c.name || ''}
Phone: ${c.phone || ''}
Address: ${c.address || ''}
City: ${c.city || ''}
Payment: Cash on Delivery`;
  return `${WA_BASE}?text=${encodeURIComponent(msg)}`;
}
