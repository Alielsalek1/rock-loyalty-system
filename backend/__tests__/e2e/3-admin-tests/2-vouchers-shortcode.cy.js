/// <reference types="cypress" />
describe('Vouchers ShortCode', () => {
    var accessToken = require('../../config.json').accessToken;
    var adminToken = require('../../config.json').adminToken;
    it('failed voucher shortcode test', () => {
        cy.request({
            method: 'GET',
            url: `/api/admin/vouchers?customerId=748&restaurantId=600&shortCode=B7A55`,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(403);
        })
    });
    it('successfull voucher shortcode test', () => {
        cy.request({
            method: 'GET',
            url: `/api/admin/vouchers?customerId=748&restaurantId=600&shortCode=B7A55`,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${adminToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(200);
            expect(response.body).to.not.be.empty;
        })
    });
})