-- KIVA BELION: loads the 8 live products into the database (safe to run more than once).
-- Names, ids and prices are copied from the website file data.js. Run this AFTER the migrations, on the real database.
-- Review every price below before you run it. No stock rows are created, so stock is not limited until you add some.

BEGIN;

INSERT INTO "Categories" ("Name","Slug","Description","SortOrder","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT 'Eau de Parfum','eau-de-parfum',NULL,1,true,now(),now()
WHERE NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Slug"='eau-de-parfum');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-NOIR-100','Noir','noir',NULL,'Our darkest, most commanding scent. Smoky oud and warm amber wrapped in a spiced base — bold enough for winter nights, refined enough for daily wear.',100,8900,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='noir');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-BLANC-100','Blanc','blanc',NULL,'A luminous white-floral built around jasmine and clean musk. Soft, confident, and easy to wear from morning to evening.',100,7200,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='blanc');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-OCEAN-100','Ocean','ocean',NULL,'Fresh marine notes over bright citrus, settling into a soft ambergris base. Our most refreshing scent — built for warm days.',100,7800,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='ocean');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-ROUGE-100','Rouge','rouge',NULL,'Deep rose and dark berries warmed with a touch of spice. Romantic and rich — one for evenings and special occasions.',100,8200,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='rouge');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-VERT-100','Vert','vert',NULL,'Crisp green notes and fig, grounded in earthy vetiver. Clean, natural, and quietly confident — our take on a modern green scent.',100,7500,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='vert');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-AMBER-100','Amber','amber',NULL,'Warm amber and soft vanilla over a woody base. Cosy and enveloping — a natural fit for evenings.',100,8500,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='amber');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-FLEUR-100','Fleur','fleur',NULL,'Soft peony and pink florals over a gentle musk base. Light, romantic, and effortless — an everyday floral that never feels heavy.',100,7400,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='fleur');

INSERT INTO "Products" ("CategoryId","Sku","Name","Slug","ShortDescription","Description","SizeMl","Price","IsActive","CreatedAtUtc","UpdatedAtUtc")
SELECT c."Id",'KB-ELEGANCE-100','Élégance','elegance',NULL,'Powdery iris and clean white musk over refined woods. Understated and sophisticated — our most versatile, unisex composition.',100,9200,true,now(),now()
FROM "Categories" c WHERE c."Slug"='eau-de-parfum'
AND NOT EXISTS (SELECT 1 FROM "Products" WHERE "Slug"='elegance');

COMMIT;

-- Check: should list 8 rows
SELECT "Slug","Name","Price","IsActive" FROM "Products" ORDER BY "Slug";
