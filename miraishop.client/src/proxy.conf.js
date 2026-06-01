const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:5292';

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
