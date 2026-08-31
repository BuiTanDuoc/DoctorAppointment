import { createContext, useEffect, useState } from "react";
import { toast } from "react-toastify";
import axios from 'axios'
import { useNavigate } from "react-router-dom";

export const AppContext = createContext()

const AppContextProvider = (props) => {

    const currencySymbol = '₹'
    const backendUrl = import.meta.env.VITE_BACKEND_URL
    const navigate = useNavigate()

    const [doctors, setDoctors] = useState([])
    const [token, setToken] = useState(localStorage.getItem('token') ? localStorage.getItem('token') : '')
    const [userData, setUserData] = useState(false)

    // If the token is invalid/expired, any user-authenticated request comes
    // back 401. Handle it in one place: clear the token and send the patient
    // to /login, and resolve with a "not successful" shape so the calling
    // function's existing `if (data.success) ... else toast.error(...)` shows
    // one friendly message instead of a raw network error.
    useEffect(() => {
        const interceptorId = axios.interceptors.response.use(
            (response) => response,
            (error) => {
                const isUserRequest = Boolean(error.config?.headers?.token)
                if (isUserRequest && error.response?.status === 401) {
                    localStorage.removeItem('token')
                    setToken('')
                    setUserData(false)
                    navigate('/login')
                    return Promise.resolve({
                        data: { success: false, message: 'Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại' }
                    })
                }
                return Promise.reject(error)
            }
        )

        return () => axios.interceptors.response.eject(interceptorId)
    }, [])

    // Getting Doctors using API
    const getDoctosData = async () => {

        try {

            const { data } = await axios.get(backendUrl + '/api/doctor/list')
            if (data.success) {
                setDoctors(data.doctors)
            } else {
                toast.error(data.message)
            }

        } catch (error) {
            console.log(error)
            toast.error(error.message)
        }

    }

    // Getting User Profile using API
    const loadUserProfileData = async () => {

        try {

            const { data } = await axios.get(backendUrl + '/api/user/get-profile', { headers: { token } })

            if (data.success) {
                setUserData(data.userData)
            } else {
                toast.error(data.message)
            }

        } catch (error) {
            console.log(error)
            toast.error(error.message)
        }

    }

    useEffect(() => {
        getDoctosData()
    }, [])

    useEffect(() => {
        if (token) {
            loadUserProfileData()
        }
    }, [token])

    const value = {
        doctors, getDoctosData,
        currencySymbol,
        backendUrl,
        token, setToken,
        userData, setUserData, loadUserProfileData
    }

    return (
        <AppContext.Provider value={value}>
            {props.children}
        </AppContext.Provider>
    )

}

export default AppContextProvider