import { env } from './env';

export const environment = {
  production: false,
  apiUrl: env.API_URL,
  defaultLanguage: env.DEFAULT_LANGUAGE,
  supportedLanguages: ['vi', 'en'],
  currentLanguage: env.DEFAULT_LANGUAGE,
  unit: env.UNIT
};
