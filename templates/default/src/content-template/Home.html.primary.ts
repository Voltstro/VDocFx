exports.transform = function (model: any): any {

    if (model.hero && model.hero.actions && model.hero.actions.length > 0) {
        model.hero.actions[0].first = true;
    }

    return model;
}