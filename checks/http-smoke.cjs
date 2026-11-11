// Node.js 22+ HTTP integration checks against a disposable local SQLite instance.
const { spawn } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const crypto = require('node:crypto');
const root = path.resolve(__dirname, '..');
const work = fs.mkdtempSync(path.join(os.tmpdir(), 'shopease-smoke-'));
const password = crypto.randomBytes(24).toString('base64') + 'Aa1!';
const base = 'http://127.0.0.1:5081';
const dotnet = process.env.SHOP_SMOKE_DOTNET || 'dotnet';
const log = fs.openSync(path.join(work, 'server.log'), 'w');
const server = spawn(dotnet, ['bin/Release/net8.0/Ecommerce.dll'], {
 cwd: path.join(root, 'ShopEase'), stdio: ['ignore', log, log],
 env: {...process.env, ASPNETCORE_URLS:base, ASPNETCORE_ENVIRONMENT:'Development',
 Database__Provider:'Sqlite', ConnectionStrings__DefaultConnection:`Data Source=${path.join(work,'shop.db')}`,
 SeedAdmin__Email:'admin@example.test', SeedAdmin__Password:password, DOTNET_HOST_PATH:dotnet}
});
const check = (ok, text) => { if (!ok) throw new Error(text); console.log('PASS: '+text); };

class Client {
 constructor(){this.cookies=new Map();}
 async request(url,method='GET',data=null){
  for(let redirects=0;redirects<10;redirects++){
   const headers={Cookie:[...this.cookies].map(([k,v])=>k+'='+v).join('; ')};
   if(data)headers['Content-Type']='application/x-www-form-urlencoded';
   const r=await fetch(new URL(url,base),{method,headers,body:data?new URLSearchParams(data):undefined,redirect:'manual'});
   for(const c of r.headers.getSetCookie()){const pair=c.split(';')[0], i=pair.indexOf('=');this.cookies.set(pair.slice(0,i),pair.slice(i+1));}
   if([301,302,303].includes(r.status)){url=r.headers.get('location');method='GET';data=null;continue;}
   return {status:r.status,text:await r.text(),url:new URL(url,base).href};
  }
  throw new Error('Redirect loop');
 }
 get(url){return this.request(url);}
 post(url,data){return this.request(url,'POST',data);}
}
function token(html){const tag=html.match(/<input[^>]*name="__RequestVerificationToken"[^>]*>/);if(!tag)throw new Error('Missing anti-forgery token');return tag[0].match(/value="([^"]+)"/)[1];}
(async () => {
 for(let i=0;i<100;i++) {
  if (server.exitCode !== null) throw new Error('Server stopped: '+fs.readFileSync(path.join(work,'server.log'),'utf8'));
  try {if ((await fetch(base+'/health')).ok) break;} catch {}
  if(i===99) throw new Error('Server not ready');
  await new Promise(r=>setTimeout(r,200));
 }

 const admin=new Client();
 let page=await admin.get('/Admin/Registry');
 check(page.url.includes('/Identity/Account/Login'),'anonymous admin access requires login');
 page=await admin.post('/Identity/Account/Login',{'Input.Email':'admin@example.test','Input.Password':password,__RequestVerificationToken:token(page.text)});
 check(!page.url.includes('/Login'),'administrator login');
 page=await admin.get('/Admin/Inventory');
 check(page.status===200 && page.text.includes('tbody'),'inventory page renders from SQLite');
 page=await admin.get('/Admin/Registry');
 check(/<dd>1<\/dd>/.test(page.text),'successful report hook updates registry');
 page=await admin.post('/Admin/Registry/Update',{deliveryNote:'Test delivery 2 days',__RequestVerificationToken:token(page.text)});
 check(page.text.includes('Test delivery 2 days'),'registry update displayed by shared layout');
 page=await admin.get('/Admin/Processes');
 await admin.post('/Admin/Processes/Start',{__RequestVerificationToken:token(page.text)});
 let status;
 for(let i=0;i<50;i++) {status=JSON.parse((await admin.get('/Admin/Processes/Status')).text);if(!['Running','Stopping'].includes(status.state))break;await new Promise(r=>setTimeout(r,100));}
 check(status.state==='Completed' && status.exitCode===0 && status.output.includes('.NET'),'external process finishes and captures output');
 const customer=new Client();
 page=await customer.get('/Identity/Account/Register');
 check(page.status===404,'public registration disabled');
 check((await customer.post('/Identity/Account/Register',{})).status===404,'registration POST disabled');
 const photos=JSON.parse((await customer.get('/images/product-images.json')).text);
 check(photos.length===54 && new Set(photos.map(p=>p.sku)).size===54,'all 54 seeded products have unique photos');
 check((await customer.get('/Home/ImageCredits')).status===200,'photo attribution page renders');
 const sample=await fetch(base+photos[0].url);
 check(sample.ok && sample.headers.get('content-type').includes('image/webp'),'product photograph served locally');
 page=await customer.get('/Admin/Registry');check(page.url.includes('/Login'),'guest cannot access admin registry');
 page=await customer.get('/Catalog?sort=price_asc');
 check(page.status===200 && page.text.includes('card-title'),'SQLite price sorting');
 check(page.text.includes('class="product-photo"'),'catalog renders product photographs');
 const prices=[...page.text.matchAll(/class="card-text fw-bold">([^<]+)/g)].map(m=>Number(m[1].replace(/&#x[0-9A-Fa-f]+;|&#[0-9]+;|[^0-9.,]/g,'').replaceAll(',','')));
 check(prices.length>1 && prices.every((p,i)=>i===0 || p>=prices[i-1]),'prices sorted numerically');
 const add=page.text.match(/href="([^"]*\/Cart\/Add[^"]*)"/)[1].replaceAll('&amp;','&');
 page=await customer.get(add);check(page.url.includes('/Cart'),'cart adds product');
 page=await customer.get('/Checkout');
 page=await customer.post('/Checkout',{Country:'Test Country',City:'Test City',Street:'Test Street 1',Zip:'12345',__RequestVerificationToken:token(page.text)});
 check(page.url.includes('/Orders/Details/') && page.text.includes('Test Street 1'),'guest checkout persists order and shipping address');
 const otherGuest=new Client();
 check((await otherGuest.get(new URL(page.url).pathname)).status===404,'guest cannot read another session order');
 page=await customer.get('/Orders');check(page.text.includes('ORD-'),'customer order history');
 page=await customer.get('/Cart');check(!page.text.includes('name="qty"'),'cart cleared after checkout');
 await customer.get(add);page=await customer.get('/Checkout');
 page=await customer.post('/Checkout',{SelectedAddressId:'999999',__RequestVerificationToken:token(page.text)});
 check(page.status===400,'invalid shipping address rejected');
 console.log('ALL HTTP CHECKS PASSED');
 console.log('QA output directory: '+work);
})().catch(e=>{console.error(e.message);process.exitCode=1;}).finally(()=>{server.kill('SIGTERM');fs.closeSync(log);});
