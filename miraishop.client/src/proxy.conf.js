const { env } = require('process');

const urls = (env.ASPNETCORE_URLS || '').split(';').map(url => url.trim()).filter(Boolean);

// Connect directly over HTTPS to keep backend redirects out of the browser.
// The fallback matches the server's "https" launch profile when started separately.
const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  urls.find(url => url.startsWith('https://')) || urls[0] || 'https://localhost:7140';

console.log('[proxy] target =', target);

const PROXY_CONFIG = [
  {
    context: ["/weatherforecast", "/api"],
    target,
    secure: false,
    logLevel: 'debug'
  }
];

module.exports = PROXY_CONFIG;
