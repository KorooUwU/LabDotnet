const assert = require('node:assert/strict');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const [blazor, razor, mvc] = (process.env.LAB_URLS || 'http://localhost:5119,http://localhost:5120,http://localhost:5130').split(',');
const stamp = Date.now();
let browser;
async function open(page, url) {
    await page.goto(url, {waitUntil: 'domcontentloaded'});
    if (url.startsWith(blazor)) await page.waitForTimeout(1200);
}
async function login(page, base, name, password, succeeds = true) {
    await open(page, base + '/Login');
    await page.locator(base === blazor ? '#login-username' : '#Input_UserName').fill(name);
    await page.locator(base === blazor ? '#login-password' : '#Input_Password').fill(password);
    if (succeeds) {
        await Promise.all([
            page.waitForURL(url => !url.pathname.toLowerCase().includes('login')),
            page.getByRole('button', {name: 'Đăng nhập', exact: true}).click()
        ]);
    } else {
        await page.getByRole('button', {name: 'Đăng nhập', exact: true}).click();
        await page.waitForTimeout(500);
        assert.ok(new URL(page.url()).pathname.toLowerCase().includes('login'));
    }
}
(async () => {
    browser = await chromium.launch({channel: 'msedge', headless: true});
    const adminContext = await browser.newContext();
    await adminContext.route('https://**/*', route => route.abort());
    const admin = await adminContext.newPage();
    admin.setDefaultTimeout(15000);
    for (const base of [razor, mvc, blazor]) {
        await open(admin, base + '/Admin/Accounts');
        assert.ok(admin.url().includes('/Login'), 'Anonymous account management must require login');
        await login(admin, base, 'admin', '123456');
    }

    // Do not run against the working database: this guard test assumes the seeded admin is the only Admin.
    await open(admin, razor + '/Admin/Accounts?Search=admin');
    await admin.locator('tbody tr').filter({hasText: 'admin@moc.local'}).getByRole('link', {name: 'Sửa quyền / mật khẩu'}).click();
    await admin.locator('input[name="RoleInput.SelectedRoles"][value="Admin"]').uncheck();
    await admin.locator('input[name="RoleInput.SelectedRoles"][value="Customer"]').check();
    await admin.getByRole('button', {name: 'Lưu quyền truy cập', exact: true}).click();
    assert.ok((await admin.locator('main').innerText()).includes('quản trị viên cuối cùng'));

    const roleName = 'AccountStaff' + stamp;
    await open(admin, razor + '/Admin/Roles');
    await admin.locator('input[name=name]').fill(roleName);
    await admin.getByRole('button', {name: 'Tạo vai trò', exact: true}).click();
    const roleForm = admin.locator('form').filter({has: admin.getByRole('heading', {name: roleName, exact: true})});
    await roleForm.locator('input[value="Admin.Access"]').check();
    await roleForm.locator('input[value="Categories.Manage"]').check();
    await roleForm.getByRole('button', {name: 'Lưu quyền', exact: true}).click();

    for (const [index, base] of [razor, mvc, blazor].entries()) {
        const username = 'account' + stamp + index;
        const password = 'NewPassword' + stamp + index;
        const targetContext = await browser.newContext();
        await targetContext.route('https://**/*', route => route.abort());
        const target = await targetContext.newPage();
        target.setDefaultTimeout(15000);
        await open(target, razor + '/Register');
        for (const [field, value] of Object.entries({
            UserName: username, FullName: 'Khách kiểm thử tài khoản ' + index,
            Email: username + '@example.com', Phone: '0901122334',
            Password: '123456', ConfirmPassword: '123456'
        })) await target.locator('#Input_' + field).fill(value);
        await target.getByRole('button', {name: 'Đăng ký', exact: true}).click();

        // A customer cannot open account administration in any lab.
        const observerBase = base === mvc ? razor : mvc;
        await login(target, observerBase, username, '123456');
        await open(target, observerBase + '/Admin/Accounts');
        assert.ok(target.url().includes('/AccessDenied'));
        await open(admin, base + '/Admin/Accounts');
        const search = admin.locator(base === blazor ? '#account-search' : '#Search');
        await search.fill(username);
        if (base !== blazor) await admin.getByRole('button', {name: 'Tìm kiếm', exact: true}).click();
        await admin.locator('tbody tr').filter({hasText: username}).getByRole('link', {name: 'Sửa quyền / mật khẩu'}).click();
        if (base === blazor) await admin.waitForTimeout(800);
        const accountUrl = admin.url();
        const userId = new URL(accountUrl).searchParams.get('id');
        const roleSelector = base === blazor ? 'input[name=selectedRoles]' : 'input[name="RoleInput.SelectedRoles"]';

        // Editing roles must work without entering a password.
        await admin.locator(roleSelector + '[value="' + roleName + '"]').check();
        await admin.getByRole('button', {name: 'Lưu quyền truy cập', exact: true}).click();
        await admin.getByRole('status').filter({hasText: 'Đã cập nhật'}).waitFor();
        assert.ok((await admin.locator('main').innerText()).includes('Đã cập nhật'));
        await login(target, observerBase, username, '123456');
        await open(target, observerBase + '/Admin/Categories');
        assert.ok(target.url().endsWith('/Admin/Categories'));
        await open(target, observerBase + '/Admin/Accounts');
        assert.ok(target.url().includes('/AccessDenied'), 'Catalog staff cannot reset passwords');

        // Valid antiforgery, insufficient permissions: a crafted reset must still be rejected.
        const loginPage = await target.request.get(observerBase + '/Login');
        const html = await loginPage.text();
        const token = html.match(/name="__RequestVerificationToken"[^>]*value="([^"]+)"/)[1];
        const unauthorized = await target.request.post(observerBase + (observerBase === razor ? '/Admin/AccountEdit?handler=Password' : '/Admin/AccountPassword'), {
            form: {__RequestVerificationToken: token, 'PasswordInput.UserId': userId, 'PasswordInput.NewPassword': password, 'PasswordInput.ConfirmPassword': password}
        });
        assert.ok(unauthorized.url().includes('/AccessDenied'));

        const newField = base === blazor ? '#new-password' : '#PasswordInput_NewPassword';
        const confirmField = base === blazor ? '#confirm-password' : '#PasswordInput_ConfirmPassword';
        await admin.locator(newField).fill(password);
        await admin.locator(confirmField).fill(password + 'Mismatch');
        await admin.getByRole('button', {name: 'Đặt lại mật khẩu', exact: true}).click();
        await admin.getByText(/Hai mật khẩu không khớp/).first().waitFor();
        assert.ok((await admin.locator('main').innerText()).includes('Hai mật khẩu không khớp'));
        // Razor/MVC response must not replay passwords into value attributes.
        if (base !== blazor) assert.ok(!(await admin.content()).includes('value="' + password + '"'));

        await admin.locator(newField).fill(password);
        await admin.locator(confirmField).fill(password);
        await admin.getByRole('button', {name: 'Đặt lại mật khẩu', exact: true}).click();
        await admin.getByRole('status').filter({hasText: 'Đã đặt lại mật khẩu'}).waitFor();
        assert.ok((await admin.locator('main').innerText()).includes('Đã đặt lại mật khẩu'));
        await open(target, observerBase + '/Admin/Categories');
        assert.ok(target.url().includes('/Login'), 'Password reset must invalidate existing cookies');
        await login(target, observerBase, username, '123456', false);
        await login(target, observerBase, username, password);
        console.log('PASS account roles, reset, validation, cross-lab password and authorization: ' + base);
        await targetContext.close();
    }
    console.log('PASS account administration for all 3 labs; last-admin protection verified.');
})().catch(error => {console.error(error);process.exitCode=1;}).finally(async () => {if (browser) await browser.close();});


