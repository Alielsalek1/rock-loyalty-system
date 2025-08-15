/// <reference types="cypress" />
describe('Vouchers', () => {
    const accessToken = require('../../config.json').accessToken;
    it('failed voucher generation test', () => {
        const body = {
            points: Infinity
        };
        cy.request({
            method: 'POST',
            url: `/api/vouchers`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            expect(response.status).to.equal(400);
        })
    });
    it('successfull voucher generation test', () => {
        const body = {
            points: 100
        };
        cy.request({
            method: 'POST',
            url: `/api/vouchers`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            expect(response.status).to.equal(201);
        })
    });
    it('successfull getting all voucher', () => {
        cy.request({
            method: 'GET',
            url: `/api/vouchers`,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            expect(response.status).to.equal(200);
        })
    });
})