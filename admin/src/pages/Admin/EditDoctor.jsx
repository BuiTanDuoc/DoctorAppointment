import React, { useContext, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { assets } from '../../assets/assets'
import { toast } from 'react-toastify'
import { AdminContext } from '../../context/AdminContext'

const EditDoctor = () => {

    const { docId } = useParams()
    const navigate = useNavigate()
    const { doctors, getAllDoctors, updateDoctor, aToken } = useContext(AdminContext)

    const [docImg, setDocImg] = useState(false)
    const [name, setName] = useState('')
    const [experience, setExperience] = useState('1 Year')
    const [fees, setFees] = useState('')
    const [about, setAbout] = useState('')
    const [speciality, setSpeciality] = useState('General physician')
    const [degree, setDegree] = useState('')
    const [address1, setAddress1] = useState('')
    const [address2, setAddress2] = useState('')
    const [available, setAvailable] = useState(true)
    const [existingImage, setExistingImage] = useState('')
    const [loaded, setLoaded] = useState(false)

    // Doctors list may not be populated yet if this page was opened via a direct
    // link/refresh rather than by clicking through from /doctor-list.
    useEffect(() => {
        if (aToken && doctors.length === 0) {
            getAllDoctors()
        }
    }, [aToken])

    useEffect(() => {
        const doctor = doctors.find(d => d._id === docId)
        if (doctor) {
            setName(doctor.name)
            setExperience(doctor.experience)
            setFees(doctor.fees)
            setAbout(doctor.about)
            setSpeciality(doctor.speciality)
            setDegree(doctor.degree)
            setAddress1(doctor.address?.line1 || '')
            setAddress2(doctor.address?.line2 || '')
            setAvailable(doctor.available)
            setExistingImage(doctor.image)
            setLoaded(true)
        }
    }, [doctors, docId])

    const onSubmitHandler = async (event) => {
        event.preventDefault()

        try {
            const formData = new FormData()

            formData.append('docId', docId)
            formData.append('name', name)
            formData.append('experience', experience)
            formData.append('fees', Number(fees))
            formData.append('about', about)
            formData.append('speciality', speciality)
            formData.append('degree', degree)
            formData.append('address', JSON.stringify({ line1: address1, line2: address2 }))
            formData.append('available', available)
            if (docImg) {
                formData.append('image', docImg)
            }

            const success = await updateDoctor(formData)
            if (success) {
                navigate('/doctor-list')
            }

        } catch (error) {
            toast.error(error.message)
            console.log(error)
        }
    }

    if (!loaded) {
        return <div className='m-5'>Loading doctor...</div>
    }

    return (
        <form onSubmit={onSubmitHandler} className='m-5 w-full'>

            <p className='mb-3 text-lg font-medium'>Edit Doctor</p>

            <div className='bg-white px-8 py-8 border rounded w-full max-w-4xl max-h-[80vh] overflow-y-scroll'>
                <div className='flex items-center gap-4 mb-8 text-gray-500'>
                    <label htmlFor="doc-img">
                        <img className='w-16 h-16 object-cover bg-gray-100 rounded-full cursor-pointer' src={docImg ? URL.createObjectURL(docImg) : existingImage || assets.upload_area} alt="" />
                    </label>
                    <input onChange={(e) => setDocImg(e.target.files[0])} type="file" name="" id="doc-img" hidden />
                    <p>Click photo to change it <br /> (leave as-is to keep current photo)</p>
                </div>

                <div className='flex flex-col lg:flex-row items-start gap-10 text-gray-600'>

                    <div className='w-full lg:flex-1 flex flex-col gap-4'>

                        <div className='flex-1 flex flex-col gap-1'>
                            <p>Doctor name</p>
                            <input onChange={e => setName(e.target.value)} value={name} className='border rounded px-3 py-2' type="text" placeholder='Name' required />
                        </div>

                        <div className='flex-1 flex flex-col gap-1'>
                            <p>Experience</p>
                            <select onChange={e => setExperience(e.target.value)} value={experience} className='border rounded px-2 py-2' >
                                <option value="1 Year">1 Year</option>
                                <option value="2 Year">2 Years</option>
                                <option value="3 Year">3 Years</option>
                                <option value="4 Year">4 Years</option>
                                <option value="5 Year">5 Years</option>
                                <option value="6 Year">6 Years</option>
                                <option value="8 Year">8 Years</option>
                                <option value="9 Year">9 Years</option>
                                <option value="10 Year">10 Years</option>
                            </select>
                        </div>

                        <div className='flex-1 flex flex-col gap-1'>
                            <p>Fees</p>
                            <input onChange={e => setFees(e.target.value)} value={fees} className='border rounded px-3 py-2' type="number" placeholder='Doctor fees' required />
                        </div>

                        <div className='flex-1 flex items-center gap-2'>
                            <input onChange={e => setAvailable(e.target.checked)} checked={available} type="checkbox" id="available" />
                            <label htmlFor="available">Available for booking</label>
                        </div>

                    </div>

                    <div className='w-full lg:flex-1 flex flex-col gap-4'>

                        <div className='flex-1 flex flex-col gap-1'>
                            <p>Speciality</p>
                            <select onChange={e => setSpeciality(e.target.value)} value={speciality} className='border rounded px-2 py-2'>
                                <option value="General physician">General physician</option>
                                <option value="Gynecologist">Gynecologist</option>
                                <option value="Dermatologist">Dermatologist</option>
                                <option value="Pediatricians">Pediatricians</option>
                                <option value="Neurologist">Neurologist</option>
                                <option value="Gastroenterologist">Gastroenterologist</option>
                            </select>
                        </div>

                        <div className='flex-1 flex flex-col gap-1'>
                            <p>Degree</p>
                            <input onChange={e => setDegree(e.target.value)} value={degree} className='border rounded px-3 py-2' type="text" placeholder='Degree' required />
                        </div>

                        <div className='flex-1 flex flex-col gap-1'>
                            <p>Address</p>
                            <input onChange={e => setAddress1(e.target.value)} value={address1} className='border rounded px-3 py-2' type="text" placeholder='Address 1' required />
                            <input onChange={e => setAddress2(e.target.value)} value={address2} className='border rounded px-3 py-2' type="text" placeholder='Address 2' required />
                        </div>

                    </div>

                </div>

                <div>
                    <p className='mt-4 mb-2'>About Doctor</p>
                    <textarea onChange={e => setAbout(e.target.value)} value={about} className='w-full px-4 pt-2 border rounded' rows={5} placeholder='write about doctor'></textarea>
                </div>

                <div className='flex gap-3'>
                    <button type='submit' className='bg-primary px-10 py-3 mt-4 text-white rounded-full'>Save changes</button>
                    <button type='button' onClick={() => navigate('/doctor-list')} className='border px-10 py-3 mt-4 rounded-full'>Cancel</button>
                </div>

            </div>

        </form>
    )
}

export default EditDoctor
