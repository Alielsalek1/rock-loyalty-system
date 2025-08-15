/// <reference types="cypress" />
describe('Points', () => {
    const accessToken = require('../../config.json').accessToken;
    it('successfull getting points test', () => {
        cy.request({
            method: 'GET',
            url: `/api/credit-points`,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(200);
            cy.log('Api response: ' + JSON.stringify(response.body));
        })
    });
    it('failed getting points test', () => {
        cy.request({
            method: 'GET',
            url: `/api/credit-points`,
            headers: {
                'Content-Type': 'application/json',
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(401);
        })
    });
    it('successfull spending points test', () => { 
        cy.request({
            method: 'POST',
            url: `/api/vouchers`,
            body: {
                points: 100
            },
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        })

    });
})