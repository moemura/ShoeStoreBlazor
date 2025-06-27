const https = require('https');

// Disable SSL verification for testing
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;

function makeRequest(method, path, data = null) {
  return new Promise((resolve, reject) => {
    const options = {
      hostname: 'localhost',
      port: 5001,
      path: path,
      method: method,
      headers: {
        'Content-Type': 'application/json',
      }
    };

    const req = https.request(options, (res) => {
      let body = '';
      res.on('data', (chunk) => {
        body += chunk;
      });
      res.on('end', () => {
        try {
          const parsedBody = JSON.parse(body);
          resolve(parsedBody);
        } catch (e) {
          resolve(body);
        }
      });
    });

    req.on('error', (e) => {
      reject(e);
    });

    if (data) {
      req.write(JSON.stringify(data));
    }

    req.end();
  });
}

async function debug() {
  try {
    console.log('🧹 Clearing caches...');
    const clearResult = await makeRequest('POST', '/api/Tests/clear-promotion-cache');
    console.log('Cache clear result:', clearResult);

    console.log('\n📋 Getting ALL active promotions...');
    const promotions = await makeRequest('GET', '/api/Promotions');
    console.log('Active promotions:', promotions.length);
    
    if (promotions && promotions.length > 0) {
      promotions.forEach((promo, index) => {
        console.log(`\n${index + 1}. ${promo.name} (Priority: ${promo.priority})`);
        console.log(`   - Type: ${promo.type}, Value: ${promo.discountValue}`);
        console.log(`   - Scope: ${promo.scope}`);
        console.log(`   - MinOrderAmount: ${promo.minOrderAmount || 'null'}`);
        console.log(`   - BrandIds: ${promo.brandIds ? promo.brandIds.join(', ') : 'null'}`);
        console.log(`   - ProductIds: ${promo.productIds ? promo.productIds.join(', ') : 'null'}`);
        console.log(`   - CategoryIds: ${promo.categoryIds ? promo.categoryIds.join(', ') : 'null'}`);
      });
    }

    console.log('\n🏷️ Getting brands...');
    const brands = await makeRequest('GET', '/api/Brands');
    console.log('Brands:');
    if (brands && brands.length > 0) {
      brands.forEach((brand, index) => {
        console.log(`${index + 1}. ${brand.name} (ID: ${brand.id})`);
      });
    }

    console.log('\n🛍️ Getting products with dynamic pricing (orderTotal=1000000)...');
    const products = await makeRequest('GET', '/api/Products/filter?pageIndex=1&pageSize=5&orderTotal=1000000');
    console.log('Products with dynamic pricing:');
    
    if (products.data && products.data.length > 0) {
      products.data.forEach((product, index) => {
        console.log(`\n${index + 1}. ${product.name} (Brand: ${product.brandName || 'N/A'}, BrandID: ${product.brandId || 'N/A'})`);
        console.log(`   - Price: ${product.price} VND`);
        console.log(`   - SalePrice: ${product.salePrice || 'null'} VND`);
        console.log(`   - PromotionPrice: ${product.promotionPrice || 'null'} VND`);
        console.log(`   - PromotionName: ${product.promotionName || 'null'}`);
        console.log(`   - PromotionDiscount: ${product.promotionDiscount || 'null'} VND`);
        console.log(`   - HasActivePromotion: ${product.hasActivePromotion || false}`);
      });
      
      // Test specific Nike product promotions
      const nikeProducts = products.data.filter(p => p.brandName === 'Nike');
      if (nikeProducts.length > 0) {
        console.log(`\n🔍 Testing Nike product promotions for: ${nikeProducts[0].name}`);
        const nikePromotions = await makeRequest('GET', `/api/Products/${nikeProducts[0].id}/promotions?orderTotal=1000000`);
        console.log('Nike product promotions:', nikePromotions.length);
        if (nikePromotions && nikePromotions.length > 0) {
          nikePromotions.forEach((promo, index) => {
            console.log(`${index + 1}. ${promo.name} (Priority: ${promo.priority}, Scope: ${promo.scope}, Discount: ${promo.discountValue})`);
          });
        }
      }
    }

  } catch (error) {
    console.error('Error:', error.message);
  }
}

debug(); 