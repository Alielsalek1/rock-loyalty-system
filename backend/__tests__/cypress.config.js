const { defineConfig } = require("cypress");
const { launchUrl } = require('./config.js');

module.exports = defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
    specPattern: [
      "e2e/**/*.{cy,spec}.{js,ts,jsx,tsx}",
    ],
    baseUrl: launchUrl,
    experimentalRunAllSpecs: true
  },
});
