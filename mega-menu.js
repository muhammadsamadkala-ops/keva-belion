/* ==========================================================================
   KIVA BELION — Mega menu (Shop / Collections)
   Plain JavaScript, no libraries. Geometry (menu size, shared highlight,
   active indicator) is driven by small spring simulations so every move
   can be interrupted and continues smoothly. Content fades/slides via CSS.
   Menu content lives in partials.js (MEGA_MENU); product cards use data.js.
   If anything here fails, the normal links in the navbar still work.
   ========================================================================== */
(function(){
  'use strict';
  try{ init(); }catch(err){ /* progressive enhancement only */ }

  function init(){
    var header = document.querySelector('header.site-nav');
    var nav = document.getElementById('navLinks');
    var mega = document.getElementById('mega');
    if(!header || !nav || !mega) return;

    var clip = mega.querySelector('.mega-clip');
    var surface = mega.querySelector('.mega-surface');
    var pill = nav.querySelector('.nav-pill');
    var ind = nav.querySelector('.nav-indicator');
    var links = Array.prototype.slice.call(nav.querySelectorAll('.nav-link'));
    var triggers = Array.prototype.slice.call(nav.querySelectorAll('.nav-trigger'));
    var panels = {};
    Array.prototype.forEach.call(mega.querySelectorAll('.mega-panel'), function(p){ panels[p.getAttribute('data-panel')] = p; });
    var ORDER = ['shop','collections'];
    var mqMobile = window.matchMedia('(max-width:900px)');
    var mqReduce = window.matchMedia('(prefers-reduced-motion: reduce)');

    /* ---------- product cards (from data.js) ---------- */
    function fillCards(){
      if(typeof getProductById !== 'function' || typeof MEGA_MENU === 'undefined') return;
      Array.prototype.forEach.call(document.querySelectorAll('[data-mega-cards]'), function(box){
        var key = box.getAttribute('data-mega-cards');
        var ids = (MEGA_MENU[key] && MEGA_MENU[key].featured) || [];
        var html = '', n = 0;
        ids.forEach(function(id){
          var p = getProductById(id);
          if(!p || !p.image) return;
          html += '<a class="mega-card" href="' + productUrl(p) + '" style="--i:' + n + '">' +
                  '<span class="mega-card-img"><img src="' + p.image + '" alt="" width="400" height="500" loading="lazy" decoding="async"></span>' +
                  '<span class="mega-card-name">' + p.name + '</span>' +
                  '<span class="mega-card-notes">' + (p.notes || '') + '</span></a>';
          n++;
        });
        box.innerHTML = html;
        box.setAttribute('data-count', String(n || 1));
      });
    }
    fillCards();

    /* ---------- springs ---------- */
    function Spring(x, k, c, prec){ this.x = x; this.t = x; this.v = 0; this.k = k; this.c = c; this.p = prec; }
    Spring.prototype.snap = function(t){ this.x = this.t = t; this.v = 0; };
    Spring.prototype.to = function(t){ if(mqReduce.matches) this.snap(t); else this.t = t; };
    Spring.prototype.step = function(dt){
      var a = this.k * (this.t - this.x) - this.c * this.v;
      this.v += a * dt; this.x += this.v * dt;
    };
    Spring.prototype.rest = function(){ return Math.abs(this.x - this.t) < this.p && Math.abs(this.v) < this.p * 6; };

    var S = {
      open: new Spring(0, 300, 27, 0.0015),
      w:    new Spring(0, 240, 28, 0.2),
      h:    new Spring(0, 240, 28, 0.2),
      px:   new Spring(0, 520, 38, 0.1),
      pw:   new Spring(0, 520, 38, 0.1),
      po:   new Spring(0, 420, 36, 0.01),
      ix:   new Spring(0, 420, 34, 0.1),
      io:   new Spring(0, 420, 36, 0.01)
    };
    var raf = 0, last = 0;
    function clamp(v, a, b){ return v < a ? a : (v > b ? b : v); }

    function apply(){
      var o = S.open.x;
      surface.style.opacity = clamp(o, 0, 1);
      surface.style.transform = 'translateY(' + ((1 - o) * -12).toFixed(2) + 'px) scale(' + (0.96 + 0.04 * o).toFixed(4) + ')';
      clip.style.width = Math.max(S.w.x, 0).toFixed(1) + 'px';
      clip.style.height = Math.max(S.h.x, 0).toFixed(1) + 'px';
      pill.style.transform = 'translateX(' + S.px.x.toFixed(2) + 'px)';
      pill.style.width = Math.max(S.pw.x, 0).toFixed(2) + 'px';
      pill.style.opacity = clamp(S.po.x, 0, 1);
      ind.style.transform = 'translateX(' + S.ix.x.toFixed(2) + 'px)';
      ind.style.opacity = clamp(S.io.x, 0, 1);
    }
    function allRest(){ for(var k in S){ if(!S[k].rest()) return false; } return true; }
    function frame(ts){
      var dt = Math.min((ts - last) / 1000, 1 / 30); last = ts;
      var sub = Math.ceil(dt * 240), h = dt / (sub || 1);
      for(var i = 0; i < sub; i++){ for(var k in S) S[k].step(h); }
      apply();
      if(allRest()){
        for(var j in S) S[j].x = S[j].t;
        apply(); raf = 0; settled(); return;
      }
      raf = requestAnimationFrame(frame);
    }
    function start(){
      if(mqReduce.matches){ for(var k in S) S[k].x = S[k].t; apply(); settled(); return; }
      if(!raf){ last = performance.now(); raf = requestAnimationFrame(frame); }
    }
    function settled(){
      if(state.key === null && S.open.t === 0){
        mega.classList.remove('is-open'); mega.setAttribute('aria-hidden', 'true');
        for(var k in panels) panels[k].classList.remove('is-active', 'is-leaving');
        state.shown = null;
      }
    }

    /* ---------- state ---------- */
    var state = { key: null, shown: null, closeTimer: 0, intentTimer: 0, lastPT: null };

    function rectIn(el){
      var a = el.getBoundingClientRect(), b = nav.getBoundingClientRect();
      return { x: a.left - b.left, w: a.width };
    }
    function triggerFor(key){ return nav.querySelector('.nav-trigger[data-menu="' + key + '"]'); }

    function restLink(){
      var path = (location.pathname.split('/').pop() || 'index.html');
      if(path === 'index.html') return nav.querySelector('[data-nav="home"]');
      if(/^(signature-collection|product)\.html$/.test(path) || /-eau-de-parfum\.html$/.test(path)) return triggerFor('shop');
      return null;
    }
    function placeIndicator(el, instant){
      if(!el || mqMobile.matches){ S.io.to(0); start(); return; }
      var r = rectIn(el), x = r.x + r.w / 2 - 11;
      if(instant || S.io.x < 0.05){ S.ix.snap(x); }
      S.ix.to(x); S.io.to(1); start();
    }
    function moveHighlight(el){
      if(!el || mqMobile.matches) return;
      var r = rectIn(el);
      if(S.po.x < 0.05){ S.px.snap(r.x); S.pw.snap(r.w); }
      S.px.to(r.x); S.pw.to(r.w); S.po.to(1); start();
    }
    function hideHighlight(){ S.po.to(0); start(); }

    function measure(key){
      var p = panels[key];
      var avail = mega.clientWidth - 32;
      var W = Math.min(parseInt(p.getAttribute('data-w'), 10) || 820, avail);
      p.style.setProperty('--w', W + 'px');
      return { W: W, H: p.offsetHeight };
    }

    function cancelClose(){ clearTimeout(state.closeTimer); state.closeTimer = 0; }
    function scheduleClose(ms){ cancelClose(); state.closeTimer = setTimeout(closeMenu, ms); }

    function openMenu(key){
      if(mqMobile.matches || !panels[key]) return;
      cancelClose(); clearTimeout(state.intentTimer);
      if(state.key === key) return;
      var g = measure(key);
      var visible = S.open.x > 0.05 && state.shown;
      var dir = 0;
      if(visible && state.shown !== key){
        dir = ORDER.indexOf(key) > ORDER.indexOf(state.shown) ? 1 : -1;
      }
      mega.style.setProperty('--dir', dir);
      mega.classList.add('is-open'); mega.setAttribute('aria-hidden', 'false');
      if(!visible){
        // grow out of the navbar: start a little smaller than the target size
        S.w.snap(g.W * 0.9); S.h.snap(g.H * 0.78);
        Object.keys(panels).forEach(function(k){ panels[k].classList.remove('is-active', 'is-leaving'); });
      } else if(state.shown && state.shown !== key){
        panels[state.shown].classList.remove('is-active');
        panels[state.shown].classList.add('is-leaving');
        (function(old){ setTimeout(function(){ if(state.shown !== old) panels[old].classList.remove('is-leaving'); }, 320); })(state.shown);
      }
      panels[key].classList.remove('is-leaving');
      panels[key].classList.add('is-active');
      state.key = key; state.shown = key;
      S.w.to(g.W); S.h.to(g.H); S.open.to(1);
      triggers.forEach(function(t){ t.setAttribute('aria-expanded', t.getAttribute('data-menu') === key ? 'true' : 'false'); t.setAttribute('aria-controls', ''); });
      var trg = triggerFor(key);
      if(trg){ trg.setAttribute('aria-controls', 'mega-' + key); panels[key].id = 'mega-' + key; moveHighlight(trg); placeIndicator(trg); }
      start();
    }

    function closeMenu(){
      cancelClose(); clearTimeout(state.intentTimer);
      if(state.key === null && S.open.t === 0) return;
      state.key = null;
      S.open.to(0);
      triggers.forEach(function(t){ t.setAttribute('aria-expanded', 'false'); });
      placeIndicator(restLink());
      start();
    }

    /* ---------- desktop pointer behaviour ---------- */
    function isMouse(e){ return e.pointerType === 'mouse' || e.pointerType === 'pen'; }

    header.addEventListener('pointerenter', function(e){ if(isMouse(e) && !mqMobile.matches) cancelClose(); });
    header.addEventListener('pointerleave', function(e){
      if(!isMouse(e) || mqMobile.matches) return;
      clearTimeout(state.intentTimer);
      scheduleClose(180);
      hideHighlight();
    });
    mega.addEventListener('pointerenter', function(e){
      if(!isMouse(e) || mqMobile.matches) return;
      cancelClose();
      var t = state.key && triggerFor(state.key); if(t) moveHighlight(t);
    });

    links.forEach(function(el){
      el.addEventListener('pointerenter', function(e){
        if(!isMouse(e) || mqMobile.matches) return;
        cancelClose(); moveHighlight(el);
        var key = el.getAttribute('data-menu');
        clearTimeout(state.intentTimer);
        if(key){
          if(state.key) openMenu(key);                       // switching: immediately
          else state.intentTimer = setTimeout(function(){ openMenu(key); }, 70);   // small hover-intent delay
        } else if(state.key){
          scheduleClose(140);
        }
      });
      el.addEventListener('pointerleave', function(){ clearTimeout(state.intentTimer); });
      el.addEventListener('pointerdown', function(e){ state.lastPT = e.pointerType; });
      el.addEventListener('focus', function(){ if(!mqMobile.matches) moveHighlight(el); });
    });

    /* ---------- click / tap / keyboard on the two triggers ---------- */
    triggers.forEach(function(t){
      var key = t.getAttribute('data-menu');
      t.addEventListener('click', function(){
        var pt = state.lastPT; state.lastPT = null;
        if(mqMobile.matches){ toggleAccordion(key); return; }
        if(pt === 'mouse' || pt === 'pen'){ openMenu(key); return; }          // hover already opened it; a click keeps it open
        if(state.key === key){ closeMenu(); return; }                       // touch / keyboard: toggle
        openMenu(key);
        if(pt === null) focusFirst(key);                                    // keyboard: move into the menu
      });
      t.addEventListener('keydown', function(e){
        state.lastPT = null;
        if(mqMobile.matches) return;
        if(e.key === 'ArrowDown'){ e.preventDefault(); openMenu(key); focusFirst(key); }
        else if(e.key === 'Escape' && state.key){ e.preventDefault(); closeMenu(); }
        else if(e.key === 'ArrowRight' || e.key === 'ArrowLeft'){
          var i = links.indexOf(t) + (e.key === 'ArrowRight' ? 1 : -1);
          if(i >= 0 && i < links.length){ e.preventDefault(); links[i].focus(); var k2 = links[i].getAttribute('data-menu'); if(state.key && k2) openMenu(k2); }
        }
      });
    });
    links.forEach(function(el){
      if(el.classList.contains('nav-trigger')) return;
      el.addEventListener('keydown', function(e){
        if(mqMobile.matches) return;
        if(e.key === 'Escape' && state.key){ closeMenu(); }
        else if(e.key === 'ArrowRight' || e.key === 'ArrowLeft'){
          var i = links.indexOf(el) + (e.key === 'ArrowRight' ? 1 : -1);
          if(i >= 0 && i < links.length){ e.preventDefault(); links[i].focus(); }
        }
      });
    });

    function focusables(key){
      return Array.prototype.slice.call(panels[key].querySelectorAll('a[href]'));
    }
    function focusFirst(key){ var f = focusables(key); if(f.length) f[0].focus(); }

    mega.addEventListener('keydown', function(e){
      if(!state.key) return;
      var list = focusables(state.key), i = list.indexOf(document.activeElement);
      if(e.key === 'Escape'){
        e.preventDefault(); var t = triggerFor(state.key); closeMenu(); if(t) t.focus();
      } else if(e.key === 'ArrowDown' || e.key === 'ArrowRight'){
        e.preventDefault(); list[(i + 1) % list.length].focus();
      } else if(e.key === 'ArrowUp' || e.key === 'ArrowLeft'){
        e.preventDefault(); list[(i - 1 + list.length) % list.length].focus();
      } else if(e.key === 'Home'){ e.preventDefault(); list[0].focus(); }
      else if(e.key === 'End'){ e.preventDefault(); list[list.length - 1].focus(); }
      else if(e.key === 'Tab'){
        if(e.shiftKey && i === 0){ e.preventDefault(); var tr = triggerFor(state.key); if(tr) tr.focus(); }
        else if(!e.shiftKey && i === list.length - 1){ closeMenu(); }
      }
    });
    header.addEventListener('focusout', function(e){
      if(mqMobile.matches) return;
      if(e.relatedTarget && !header.contains(e.relatedTarget)){ closeMenu(); hideHighlight(); }
    });
    document.addEventListener('pointerdown', function(e){
      if(mqMobile.matches || !state.key) return;
      if(!header.contains(e.target)){ closeMenu(); hideHighlight(); }
    });
    document.addEventListener('keydown', function(e){
      if(e.key === 'Escape' && state.key && !mega.contains(document.activeElement)) closeMenu();
    });

    /* ---------- mobile / tablet accordion ---------- */
    function toggleAccordion(key){
      var items = nav.querySelectorAll('.nav-item.has-menu');
      Array.prototype.forEach.call(items, function(it){
        var on = it.getAttribute('data-key') === key && !it.classList.contains('is-open');
        it.classList.toggle('is-open', on);
        it.querySelector('.nav-trigger').setAttribute('aria-expanded', on ? 'true' : 'false');
      });
    }
    function resetAccordion(){
      Array.prototype.forEach.call(nav.querySelectorAll('.nav-item.has-menu'), function(it){
        it.classList.remove('is-open'); it.querySelector('.nav-trigger').setAttribute('aria-expanded', 'false');
      });
    }

    /* ---------- layout refresh (resize, fonts, breakpoint) ---------- */
    function refresh(){
      if(mqMobile.matches){ return; }
      if(state.key){
        var g = measure(state.key); S.w.snap(g.W); S.h.snap(g.H);
        var t = triggerFor(state.key); if(t){ var r = rectIn(t); S.px.snap(r.x); S.pw.snap(r.w); S.ix.snap(r.x + r.w / 2 - 11); }
        apply();
      } else {
        var rl = restLink(); if(rl){ var rr = rectIn(rl); S.ix.snap(rr.x + rr.w / 2 - 11); S.io.snap(1); apply(); }
      }
    }
    var rt = 0;
    window.addEventListener('resize', function(){ clearTimeout(rt); rt = setTimeout(refresh, 120); });
    var onBp = function(){
      closeMenu(); S.open.snap(0); S.po.snap(0); apply(); settled();
      mega.classList.remove('is-open'); resetAccordion(); refresh();
      if(mqMobile.matches){ S.io.snap(0); apply(); }
    };
    if(mqMobile.addEventListener) mqMobile.addEventListener('change', onBp); else mqMobile.addListener(onBp);
    if(document.fonts && document.fonts.ready) document.fonts.ready.then(refresh);

    // current page: gold indicator at rest + aria-current on Home
    var home = nav.querySelector('[data-nav="home"]');
    if(home && (location.pathname.split('/').pop() || 'index.html') === 'index.html' && !location.hash) home.setAttribute('aria-current', 'page');
    if(!mqMobile.matches){
      var rl = restLink();
      if(rl){ var rr = rectIn(rl); S.ix.snap(rr.x + rr.w / 2 - 11); S.io.snap(1); }
      apply();
    }
  }
})();
