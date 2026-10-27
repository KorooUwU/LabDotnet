const assert = require('node:assert/strict');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const urls = (process.env.LAB_URLS || 'http://localhost:5119,http://localhost:5120,http://localhost:5130').split(',');
const [blazor, razor, mvc] = urls;
const stamp = Date.now();
const username = 'smoke' + stamp;
let browser;
async function go(page, url, isBlazor = false) {
 const response = await page.goto(url, {waitUntil: 'domcontentloaded'});
 if (isBlazor) await page.waitForTimeout(1200);
 return response;
}
async function login(page, url, name, isBlazor = false) {
 await go(page, url + '/Login', isBlazor);
 await page.locator(isBlazor ? '#login-username' : '#Input_UserName').fill(name);
 await page.locator(isBlazor ? '#login-password' : '#Input_Password').fill('123456');
 await Promise.all([
  page.waitForURL(u => !u.pathname.toLowerCase().includes('login')),
  page.getByRole('button', {name: 'Đăng nhập', exact: true}).click()
 ]);
 if (isBlazor) await page.waitForTimeout(700);
}
(async () => {
 browser = await chromium.launch({channel: 'msedge', headless: true});
 const context = await browser.newContext();
 await context.route('https://**/*', route => route.abort());
 const page = await context.newPage();
 page.setDefaultTimeout(10000);
 for (const url of urls) {
  assert.equal((await go(page, url + '/')).status(), 200);
  await go(page, url + '/Admin', url === blazor);
  assert.ok(page.url().includes('/Login'), 'Anonymous admin must require login: ' + url);
 }
 await go(page, razor + '/Register');
 const registration = {
  UserName: username, FullName: 'Khách kiểm thử ' + stamp,
  Email: username + '@example.com', Phone: '0901122334',
  Password: '123456', ConfirmPassword: '123456'
 };
 for (const [field, value] of Object.entries(registration)) await page.locator('#Input_' + field).fill(value);
 await page.getByRole('button', {name: 'Đăng ký', exact: true}).click();
 assert.equal(new URL(page.url()).pathname, '/');
 await go(page, razor + '/Admin/Products');
 assert.ok(page.url().includes('/AccessDenied'));
 await go(page, razor + '/');
 await page.getByRole('button', {name: 'Thêm vào giỏ', exact: true}).first().click();
 await page.locator('input[name=quantity]').fill('2');
 await page.getByRole('button', {name: 'Cập nhật', exact: true}).click();
 assert.equal(await page.locator('.cart-count').innerText(), '2');
 await page.getByRole('link', {name: 'Thanh toán', exact: true}).click();
 await page.locator('#Input_Address').fill('Địa chỉ kiểm thử tự động');
 await page.getByRole('button', {name: 'Xác nhận đặt hàng', exact: true}).click();
 const orderId = new URL(page.url()).searchParams.get('id');
 assert.ok(orderId, 'Checkout must return an order ID');
 for (const url of [mvc, blazor]) {
  await login(page, url, username, url === blazor);
  await go(page, url + '/MyOrders?id=' + orderId, url === blazor);
  assert.equal(await page.getByRole('heading', {name: 'Đơn #' + orderId, exact: true}).count(), 1);
  await go(page, url + '/Admin/Products', url === blazor);
  assert.ok(page.url().includes('/AccessDenied') || (await page.locator('main').innerText()).includes('không có quyền'));
 }
 // MVC checkout must use the same account and database.
 await go(page, mvc + '/');
 await page.getByRole('button', {name: 'Thêm vào giỏ', exact: true}).first().click();
 await page.getByRole('link', {name: 'Thanh toán', exact: true}).click();
 await page.locator('#Input_Address').fill('Địa chỉ đơn MVC');
 await page.getByRole('button', {name: 'Xác nhận đặt hàng', exact: true}).click();
 assert.ok(new URL(page.url()).searchParams.get('id'));
 // Identity must never let a client select its own admin role during registration.
 const stranger = await context.newPage();
 await go(stranger, mvc + '/Register');
 for (const [field, value] of Object.entries({...registration, UserName: username + 'b', Email: username + 'b@example.com'})) await stranger.locator('#Input_' + field).fill(value);
 await stranger.getByRole('button', {name: 'Đăng ký', exact: true}).click();
 await go(stranger, mvc + '/MyOrders?id=' + orderId);
 assert.equal(await stranger.getByRole('heading', {name: 'Đơn #' + orderId, exact: true}).count(), 0);
 await login(page, razor, 'admin');
 await go(page, razor + '/Admin/Categories');
 await page.locator('#Input_Name').fill('Smoke ' + stamp);
 await page.locator('#Input_Description').fill('Danh mục dùng chung');
 await page.getByRole('button', {name: 'Lưu danh mục', exact: true}).click();
 for (const url of [mvc, blazor]) {
  await login(page, url, 'admin', url === blazor);
  await go(page, url + '/Admin/Categories', url === blazor);
  assert.equal(await page.locator('tbody tr').filter({hasText: 'Smoke ' + stamp}).count(), 1);
 }
 console.log('PASS: 3 labs, shared Identity/database/categories/orders, Razor + MVC checkout, admin authorization and order ownership.');
 console.log('Test account: ' + username + '; test data are retained for inspection.');
})().catch(error => {console.error(error);process.exitCode = 1;}).finally(async () => {if (browser) await browser.close();});

