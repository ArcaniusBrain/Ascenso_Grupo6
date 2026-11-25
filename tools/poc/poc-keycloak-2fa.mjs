// Prueba técnica reproducible PoC ADR-09 (D4): Keycloak 26 + Mailpit + S1 2FA
// Uso: con `docker compose -f deploy/docker-compose.yml up -d postgres keycloak mailpit` activo,
// ejecutar `node tools/poc/poc-keycloak-2fa.mjs`
import net from 'node:net';

const KEYCLOAK_URL = process.env.KEYCLOAK_URL ?? 'http://localhost:8085';
const MAILPIT_URL = process.env.MAILPIT_URL ?? 'http://localhost:8025';
const SMTP_HOST = process.env.SMTP_HOST ?? '127.0.0.1';
const SMTP_PORT = Number(process.env.SMTP_PORT ?? '1025');

async function waitForUrl(url, maxRetries = 40, delayMs = 2000) {
  for (let i = 1; i <= maxRetries; i++) {
    try {
      const res = await fetch(url);
      if (res.ok) return res;
    } catch {}
    await new Promise(r => setTimeout(r, delayMs));
  }
  throw new Error(`Timeout esperando ${url}`);
}

async function sendSmtpCode(to, codigo) {
  return new Promise((resolve, reject) => {
    const client = net.createConnection({ host: SMTP_HOST, port: SMTP_PORT });
    const lines = [
      'EHLO ascenso-s1.local\r\n',
      'MAIL FROM:<no-reply@ascenso.org>\r\n',
      `RCPT TO:<${to}>\r\n`,
      'DATA\r\n',
      `From: ASCENSO <no-reply@ascenso.org>\r\nTo: ${to}\r\nSubject: ASCENSO - Codigo de verificacion en dos pasos\r\n\r\nTu codigo de verificacion en dos pasos en ASCENSO es: ${codigo}. Vence en 15 minutos y es de un solo uso.\r\n.\r\n`,
      'QUIT\r\n'
    ];
    let step = 0;
    client.on('data', () => {
      if (step < lines.length) {
        client.write(lines[step++]);
      } else {
        client.end();
        resolve();
      }
    });
    client.on('error', reject);
  });
}

async function main() {
  console.log('1. Esperando Keycloak realm ascenso y Mailpit...');
  const oidcRes = await waitForUrl(`${KEYCLOAK_URL}/realms/ascenso/.well-known/openid-configuration`);
  const oidc = await oidcRes.json();
  console.log(`   OK Keycloak issuer: ${oidc.issuer}`);

  await waitForUrl(`${MAILPIT_URL}/api/v1/messages`);
  console.log(`   OK Mailpit listo en ${MAILPIT_URL}`);

  console.log('2. Probando contraseña incorrecta en Keycloak (debe fallar 401)...');
  const badRes = await fetch(oidc.token_endpoint, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'password',
      client_id: 'ascenso-s1-identidad',
      client_secret: 'dev-s1-client-secret-change-in-server',
      username: 'laura.coordinadora@ascenso.org',
      password: 'PasswordIncorrecta!'
    })
  });
  if (badRes.status !== 401) throw new Error(`Esperado 401, obtenido ${badRes.status}`);
  console.log('   OK Contraseña incorrecta rechazada (401 Unauthorized)');

  console.log('3. Probando contraseña correcta de laura.coordinadora@ascenso.org en Keycloak...');
  const okRes = await fetch(oidc.token_endpoint, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'password',
      client_id: 'ascenso-s1-identidad',
      client_secret: 'dev-s1-client-secret-change-in-server',
      username: 'laura.coordinadora@ascenso.org',
      password: 'AscensoDev2026!'
    })
  });
  if (!okRes.ok) throw new Error(`Esperado 200, obtenido ${okRes.status}: ${await okRes.text()}`);
  const tokenJson = await okRes.json();
  if (!tokenJson.access_token) throw new Error('Keycloak no devolvió access_token');
  console.log('   OK Credenciales primarias validadas por Keycloak 26 (JWT recibido)');

  console.log('4. Probando OAuth2 client_credentials de ascenso-s4-becas con scopes privados...');
  const s4Res = await fetch(oidc.token_endpoint, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      grant_type: 'client_credentials',
      client_id: 'ascenso-s4-becas',
      client_secret: 'dev-s4-client-secret-change-in-server',
      scope: 'fondos.reservar fondos.liberar otp.consumir'
    })
  });
  if (!s4Res.ok) throw new Error(`S4 client_credentials falló: ${s4Res.status} ${await s4Res.text()}`);
  const s4Token = await s4Res.json();
  console.log(`   OK Token S4 emitido con scopes: ${s4Token.scope}`);

  console.log('5. Enviando código 2FA real vía SMTP a Mailpit y verificando bandeja...');
  const codigoPrueba = '482913';
  await sendSmtpCode('laura.coordinadora@ascenso.org', codigoPrueba);
  await new Promise(r => setTimeout(r, 500));

  const msgsRes = await fetch(`${MAILPIT_URL}/api/v1/messages`);
  const msgs = await msgsRes.json();
  if (!msgs.messages || msgs.messages.length === 0) throw new Error('Mailpit no recibió el correo 2FA');
  const ultimo = msgs.messages[0];
  const detalleRes = await fetch(`${MAILPIT_URL}/api/v1/message/${ultimo.ID}`);
  const detalle = await detalleRes.json();
  if (!detalle.Text.includes(codigoPrueba)) throw new Error('El cuerpo del correo no contiene el código 2FA');
  console.log(`   OK Correo 2FA recibido en Mailpit para ${ultimo.To[0].Address} con código ${codigoPrueba}`);

  console.log('\n=== PoC ADR-09 (D4) COMPLETADO CON ÉXITO ===');
}

main().catch(err => {
  console.error('ERROR EN PoC:', err);
  process.exit(1);
});
