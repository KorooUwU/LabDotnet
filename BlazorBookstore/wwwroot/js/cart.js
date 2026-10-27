window.mocCart = {
    read: () => window.sessionStorage.getItem("moc-cart"),
    write: value => window.sessionStorage.setItem("moc-cart", value)
};
