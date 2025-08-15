/// <reference types="cypress" />
describe('Restaurant Settings', () => {
    var adminToken = require('../../config.json').adminToken;
    var accessToken = require('../../config.json').accessToken;
    it('failed restaurant settings creation test', () => {
        const body = {
            restaurantId: 600,
            creditPointsBuyingRate: 5,
            creditPointsSellingRate: 3,
            loyaltyPointsBuyingRate: 0.0,
            loyaltyPointsSellingRate: 0.0,
            creditPointsLifeTime: 1000,
            loyaltyPointsLifeTime: 0,
            voucherLifeTime: 1000
        };
        cy.request({
            method: 'POST',
            url: `/api/admin/restaurants`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${accessToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(403);
        })
    });
    it('successfull restaurant settings creation test', () => {
        const body = {
            restaurantId: 600,
            creditPointsBuyingRate: 5,
            creditPointsSellingRate: 3,
            loyaltyPointsBuyingRate: 0.0,
            loyaltyPointsSellingRate: 0.0,
            creditPointsLifeTime: 1000,
            loyaltyPointsLifeTime: 0,
            voucherLifeTime: 1000
        };
        cy.request({
            method: 'POST',
            url: `/api/admin/restaurants`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${adminToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(201);
        })
    });
    it('successfull getting restaurant settings test', () => {
        cy.request({
            method: 'GET',
            url: `/api/admin/restaurants/600`,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${adminToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(200);
            expect(response.body).to.not.be.empty;
            cy.log('Api response: ' + JSON.stringify(response.body));
            Cypress.env('restaurantCreditPointsSellingRate', response.body.creditPointsBuyingRate);
        })
    });
    it('successfull updating restaurant settings test', () => {
        const body = {
            creditPointsBuyingRate: 4
        };
        cy.request({
            method: 'PUT',
            url: `/api/admin/restaurants/600`,
            body,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${adminToken}`
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(200);
            cy.request({
                method: 'GET',
                url: `/api/admin/restaurants/600`,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${adminToken}`
                },
                failOnStatusCode: false
            }).then(response => {
                expect(response.body.creditPointsBuyingRate).to.not.equal(Cypress.env('restaurantCreditPointsSellingRate'));
                cy.log('Api response: ' + JSON.stringify(response.body));
            })
        })
    });
})