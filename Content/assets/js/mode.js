'use strict'

const $theme = localStorage.getItem('theme')
document.querySelector('html').classList.add($theme)
