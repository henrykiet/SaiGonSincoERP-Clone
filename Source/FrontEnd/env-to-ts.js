const fs = require('fs');
const dotenv = require('dotenv');

const env = dotenv.config().parsed;

const content = `// Generated from .env
export const env = {
  API_URL: '${env.API_URL}',
  DEFAULT_LANGUAGE: '${env.DEFAULT_LANGUAGE}',
  UNIT: '${env.UNIT}'
};
`;

fs.writeFileSync('./src/environments/env.ts', content);
