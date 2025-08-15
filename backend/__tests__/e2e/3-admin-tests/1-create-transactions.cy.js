/// <reference types="cypress" />
describe('Transactions', () => {
    var adminToken = require('../../config.json').adminToken;
    var accessToken = require('../../config.json').accessToken;
    it('successfull transaction creation test', () => {
        const body = {
            customerId: 748,
            restaurantId: 600,
            receiptId: 0,
            amount: 70000000,
            transactionType: 0,
            transactionDate: "2024-08-28T08:59:27.873Z"
        };
        cy.request({
            method: 'POST',
            url: `/api/admin/credit-points-transactions`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${adminToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            expect(response.status).to.equal(201);
        })
    });
    it('failed transaction creation test', () => {
        const body = {
            customerId: 748,
            restaurantId: 600,
            receiptId: 0,
            amount: 70000000,
            transactionType: 0,
            transactionDate: "2024-08-28T08:59:27.873Z"
        };
        cy.request({
            method: 'POST',
            url: `/api/admin/credit-points-transactions`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            expect(response.status).to.equal(403);
        })
    });
})